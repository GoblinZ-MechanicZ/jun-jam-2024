namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.HighDefinition;

    public sealed class CameraSettingsController : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public CameraSettings _cameraSettings => GoblinGameManager.Instance.CameraSettingsVar;
        Material m_Material;
        public bool IsActive() => m_Material != null && _cameraSettings != null && _cameraSettings.Enabled;
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

        public override void Setup()
        {
            if (Shader.Find("Hidden/GoblinzMechanics/BSC") != null)
                m_Material = new Material(Shader.Find("Hidden/GoblinzMechanics/BSC"));
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (m_Material == null || _cameraSettings == null) return;

            m_Material.SetFloat(Shader.PropertyToID("_Brightness"), _cameraSettings.Brightness);
            m_Material.SetFloat(Shader.PropertyToID("_Saturation"), _cameraSettings.Saturation);
            m_Material.SetFloat(Shader.PropertyToID("_Contrast"), _cameraSettings.Contrast);
            m_Material.SetFloat(Shader.PropertyToID("_Hue"), _cameraSettings.HUE);

            cmd.Blit(source, destination, m_Material, 0);
        }

        public override void Cleanup() => CoreUtils.Destroy(m_Material);
    }
}