using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
public class LightShadow : ImageEffectBase {
	private RenderTexture shadowMap;
	private Camera lightCamera;
	private Light light;
	private VolumetricFog fog;
	private Matrix4x4 cropMatrix, originMatrix;

	void Awake() {
		light = GetComponent<Light>();

		lightCamera = GetComponent<Camera>();
		lightCamera.aspect = 1.0f;
		lightCamera.depthTextureMode = DepthTextureMode.Depth;

		shadowMap = new RenderTexture (512, 512, 0, RenderTextureFormat.RFloat);
		shadowMap.filterMode = FilterMode.Point;
		shadowMap.Create ();

		switch (light.type) {
		case LightType.Directional:
			lightCamera.orthographic = true;
			originMatrix = lightCamera.projectionMatrix;
			break;
		case LightType.Spot:
			lightCamera.orthographic = false;
			lightCamera.fieldOfView = light.spotAngle;
			break;
		}
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination) {
		if (fog) {
			if (light.type == LightType.Directional) {
				cropMatrix = cropMatrixCalc(0);
				lightCamera.projectionMatrix = cropMatrix * originMatrix;
			}
			lightCamera.targetTexture = shadowMap;
			Graphics.Blit(source, destination, material);
		}
	}

	public Matrix4x4 cropMatrixCalc(int index) {
		Matrix4x4 lightMatrix = originMatrix * lightCamera.worldToCameraMatrix;
		VolumetricFog.Frustum lightSpaceFrustum = fog.subfrustumCalc(index, lightMatrix);

		Vector4 max = new Vector4(
			lightSpaceFrustum.max.x,
			lightSpaceFrustum.max.y,
			lightSpaceFrustum.max.z,
			1.0f
		);

		Vector4 lightSpaceMax = new Vector4(
			Vector4.Dot(max, lightMatrix.GetRow(0)),
			Vector4.Dot(max, lightMatrix.GetRow(1)),
			Vector4.Dot(max, lightMatrix.GetRow(2)),
			Vector4.Dot(max, lightMatrix.GetRow(3))
		);

		Vector4 min = new Vector4(
			lightSpaceFrustum.min.x,
			lightSpaceFrustum.min.y,
			lightSpaceFrustum.min.z,
			1.0f
		);

		Vector4 lightSpaceMin = new Vector4(
			Vector4.Dot(min, lightMatrix.GetRow(0)),
			Vector4.Dot(min, lightMatrix.GetRow(1)),
			Vector4.Dot(min, lightMatrix.GetRow(2)),
			Vector4.Dot(min, lightMatrix.GetRow(3))
		);
 
		lightSpaceFrustum.min.z = -1.0f;  
 
		float scaleX, scaleY, scaleZ;  
		float offsetX, offsetY, offsetZ;  
		scaleX = 2.0f / (lightSpaceFrustum.max.x - lightSpaceFrustum.min.x);  
		scaleY = 2.0f / (lightSpaceFrustum.max.y - lightSpaceFrustum.min.y);  
		offsetX = -0.5f * (lightSpaceFrustum.max.x + lightSpaceFrustum.min.x) * scaleX;  
		offsetY = -0.5f * (lightSpaceFrustum.max.y + lightSpaceFrustum.min.y) * scaleY;  
		scaleZ = 2.0f / (lightSpaceFrustum.max.z - lightSpaceFrustum.min.z);  
		offsetZ = -0.5f * (lightSpaceFrustum.max.z + lightSpaceFrustum.min.z) * scaleZ;

		Matrix4x4 cropMatrix = new Matrix4x4();
		cropMatrix.SetRow(0, new Vector4( scaleX,     0.0f,     0.0f,  offsetX));
		cropMatrix.SetRow(1, new Vector4(   0.0f,   scaleY,     0.0f,  offsetY));
		cropMatrix.SetRow(2, new Vector4(   0.0f,     0.0f,   scaleZ,  offsetZ));
		cropMatrix.SetRow(3, new Vector4(   0.0f,     0.0f,     0.0f,     1.0f));

		return cropMatrix;  
	}

	public void setVolumetricFog(VolumetricFog fog) {
		this.fog = fog;
	}

	public Texture getShadowMap() {
		return shadowMap;
	}

	public Matrix4x4 getLightMatrix() {
		return lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix;
	}
}
