using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;
using System.Collections;

[ExecuteInEditMode]
public class VolumetricFog : ImageEffectBase {
	public ComputeShader rayMarching, lightingAndDensityCalc;

	private RenderTexture lighting, shadow, fog;
	private int width, height, volumeDepth;
	private Camera camera;
	private ComputeBuffer cameraParam, lightParam;

	private LightShadow lightShadow;
	private float[] cascadeDepth;

	public Light sunLight, flashLight;
	public float fogFar = 70.0f;
	private Light[] lights;
	private LightParam[] lightParams;

	void Awake() {
		camera = GetComponent<Camera>();
		camera.depthTextureMode = DepthTextureMode.Depth;

		width = 160;
		height = 90;
		volumeDepth = 128;

		lightShadow = sunLight.GetComponent<LightShadow>();
		lightShadow.setVolumetricFog(this);

		lighting = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
		lighting.useMipMap = true;
		lighting.dimension = TextureDimension.Tex3D;
		lighting.volumeDepth = volumeDepth;
		lighting.filterMode = FilterMode.Trilinear;
		lighting.enableRandomWrite = true;
		lighting.Create();

		fog = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
		fog.useMipMap = true;
		fog.dimension = TextureDimension.Tex3D;
		fog.volumeDepth = volumeDepth;
		fog.filterMode = FilterMode.Trilinear;
		fog.enableRandomWrite = true;
		fog.Create();

		cascadeDepth = new float[] {
			camera.nearClipPlane,
			30.0f
		};

		lights = (Light[]) GameObject.FindObjectsOfType(typeof(Light));

	}

	void OnRenderImage(RenderTexture source, RenderTexture destination) {
		cameraParam = new ComputeBuffer(1, (3 + 4 * 4 + 4) * 4);
		cameraParam.SetData(new CameraParam[] { new CameraParam(camera, fogFar) });
		lightingAndDensityCalc.SetBuffer(0, "camera", cameraParam);

		lightParams = new LightParam[lights.Length];
		for (int i = 0; i < lights.Length; i++) {
			lightParams [i] = new LightParam (lightShadow, lights [i]);
		}
		lightParam = new ComputeBuffer(lightParams.Length, (3 + 3 + 4 * 4 + 2 + 3 + 1) * 4);
		lightParam.SetData (lightParams);
		lightingAndDensityCalc.SetInt ("lightLength", lightParams.Length);
		lightingAndDensityCalc.SetBuffer(0, "light", lightParam);
		lightingAndDensityCalc.SetTexture(0, "sunShadow", lightShadow.getShadowMap());
		/*if (light.cookie) {
			lightingAndDensityCalc.SetTexture(0, "cookie", light.cookie);
		}*/
		lightingAndDensityCalc.SetTexture(0, "Result", lighting);
		lightingAndDensityCalc.SetFloat("time", Time.time);
		lightingAndDensityCalc.Dispatch(0, 10, 12, 16);

		rayMarching.SetTexture(0, "Result", fog);
		rayMarching.SetTexture(0, "Input", lighting);
		rayMarching.Dispatch(0, 5, 5, 1);

		material.shader = shader;
		material.SetTexture("_Volume", fog);
		material.SetFloat("fogFar", fogFar);
		material.SetFloat("cameraFar", camera.farClipPlane);
		Graphics.Blit(source, destination, material);

		cameraParam.Release();
		lightParam.Release();
	}

	public Frustum subfrustumCalc(int index, Matrix4x4 lightMatrix) {
		Vector3 origin;
		Vector3 direction, worldDirection;
		Vector3 position;
		Frustum subfrustum;

		origin = camera.transform.position;

		Vector2[] corner = new Vector2[] {
			new Vector2(-1, 1),
			new Vector2(1, 1),
			new Vector2(1, -1),
			new Vector2(-1, -1)
		};

		subfrustum = new Frustum();
		subfrustum.init();
		for (int i = 0; i < 4; i++) {
			direction = new Vector3 (
				camera.aspect * corner[i].x,
				1.0f * corner[i].y,
				-(1.0f / Mathf.Tan((Mathf.Deg2Rad * camera.fieldOfView) / 2.0f))
			).normalized;
			worldDirection = camera.cameraToWorldMatrix.MultiplyVector(direction);

			for (int j = 0; j < 2; j++) {
				position = origin + worldDirection * (cascadeDepth[index + j] / Mathf.Abs(direction.z));
				position = lightMatrix.MultiplyPoint (position);

				subfrustum.max = Vector3.Max(subfrustum.max, position);
				subfrustum.min = Vector3.Min(subfrustum.min, position);
			}
		}

		return subfrustum;
	}

	public struct CameraParam {
		public Vector3 pos;
		public Matrix4x4 mat;
		public float aspect;
		public float fov;
		public float near;
		public float far;

		public CameraParam(Camera camera, float fogFar) {
			pos = camera.transform.position;
			mat = camera.cameraToWorldMatrix;
			aspect = camera.aspect;
			fov = camera.fieldOfView;
			near = camera.nearClipPlane;
			far = fogFar;
		}
	}

	public struct LightParam {
		public Vector3 pos;
		public Vector3 dir;
		public Matrix4x4 mat;
		public float angle;
		public float intensity;
		public Vector3 color;
		public int type;

		public LightParam(LightShadow lightShadow, Light light) {
			pos = light.transform.position;
			dir = light.transform.forward;
			mat = lightShadow.getLightMatrix();
			angle = light.spotAngle;
			intensity = light.intensity;
			color = new Vector3(light.color.r, light.color.g, light.color.b);
			type = (int) light.type;
		}
	}

	public struct Frustum {
		public Vector3 min;
		public Vector3 max;

		public void init() {
			min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		}

		public Frustum(Vector3 min, Vector3 max) {
			this.min = min;
			this.max = max;
		}
	}
}
