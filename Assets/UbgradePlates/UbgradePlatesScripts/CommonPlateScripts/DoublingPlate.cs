using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Doubling Plate", menuName = "Inventory/Plates/Doubling Plate")]
public class DoublingPlate : UbgradePlate
{
    [SerializeField] [Range(0f, 100f)] private float chanceOfDoubling = 50;

    public override void OnApply(PlayerControll player)
    {
        player.OnBulletSpawned += ApplyEffect;
    }

    public override void OnRemove(PlayerControll player)
    {
        player.OnBulletSpawned -= ApplyEffect;
    }

    async private void ApplyEffect(Bullet originalBullet)
    {
        if (originalBullet.isDuplicate) return;

        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= chanceOfDoubling)
        {
            await Task.Delay(100);
            GameObject duplicatedObj = Instantiate(
                originalBullet.gameObject, 
                originalBullet.transform.position, 
                originalBullet.transform.rotation
            );
            
            if (duplicatedObj.TryGetComponent(out Bullet newBullet))
            {
                newBullet.isDuplicate = true;
            }
        }
    }
}