using UnityEngine;

public class ObstacleTypeB : ObstacleBase
{
    // �θ� Ŭ������ ���� ������ ����� ������ ���� �ʽ��ϴ�.
    public override void OnPlayerHit(FallingPlayer player)
    {
        // 3�� �ӵ��� 3�ʰ� ����
        if (FallGameManager.instance != null)
        {
            FallGameManager.instance.SpeedUpWakeRateTemp(3f, 3f);
        }
        if (CameraShaker.instance != null)
        {
            CameraShaker.instance.Shake(3f, 0.3f); // 0.1�� ����, 0.2 ����� �̼��ϰ� ���ϴ�.
            
        }
        Debug.Log("�˶�! ���� �ߵ�");
        Destroy(gameObject);
    }
}