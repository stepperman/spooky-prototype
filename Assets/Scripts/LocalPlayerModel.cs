using UnityEngine;
using System.Linq;
using Ludiq.OdinSerializer.Utilities;

public class LocalPlayerModel : MonoBehaviour
{
    [SerializeField] private GameObject[] networkedObject;
    public void SetActive(bool active) => networkedObject?.ForEach(x => x.SetActive(false));
}
