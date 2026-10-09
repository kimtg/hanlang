using System;

namespace HanlangGame.Engine
{
    /// <summary>
    /// 2.5D 의사 3D 원근(Perspective) 투영 엔진
    /// 화면 아래쪽(가까운 곳)이 더 빠르고 크게 이동하도록 수학적 원근 투영을 제공합니다.
    /// </summary>
    public static class Perspective
    {
        // 지면 가상 좌표 범위 (VirtualWidth = 320, VirtualHeight = 240)
        public const float HorizonY = 140f;   // 지평선 / 도로 상단 시작점
        public const float BottomY = 240f;    // 화면 최하단
        public const float CenterX = 160f;    // 소실점 중심 X (가상 화면 중앙 160)

        // 원근 계수: 화면 위쪽(원경)은 느리게, 화면 아래쪽(근경)은 빠르게
        // 상단 0.72배 대비 하단 1.28배로 화면 아래쪽이 약 1.78배 빠르게 이동
        public const float MinScale = 0.72f;  // 지평선 부근 (원경 건축물 0.6배와 부드럽게 연결)
        public const float MaxScale = 1.28f;  // 화면 최하단

        /// <summary>
        /// Y 깊이에 따른 원근 계수 반환 (0.72 ~ 1.28)
        /// </summary>
        public static float GetScale(float y)
        {
            float t = Math.Clamp((y - HorizonY) / (BottomY - HorizonY), 0f, 1f);
            return MinScale + (MaxScale - MinScale) * t;
        }

        /// <summary>
        /// 월드 X 및 Y 좌표를 원근 투영이 적용된 가상 화면 X 좌표로 변환
        /// </summary>
        public static float ProjectX(float worldX, float cameraX, float y)
        {
            float p = GetScale(y);
            float relX = worldX - cameraX;
            return CenterX + (relX - CenterX) * p;
        }

        /// <summary>
        /// 960x720 고해상도 캔버스 X 좌표 직접 반환
        /// </summary>
        public static float ProjectScreenX(float worldX, float cameraX, float y)
        {
            return ProjectX(worldX, cameraX, y) * PixelBuffer.Scale;
        }

        /// <summary>
        /// 960x720 고해상도 캔버스 Y 좌표 반환 (높이 Z 고려)
        /// </summary>
        public static float ProjectScreenY(float worldY, float worldZ = 0f)
        {
            return (worldY - worldZ) * PixelBuffer.Scale;
        }
    }
}
