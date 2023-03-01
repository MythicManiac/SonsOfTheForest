using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FMODUnity;
using UnityEngine;

public class ThomasController : MonoBehaviour
{
    public Transform target;
    public float speed = 100;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        var bundle = AssetBundle.LoadFromFile(Path.Join(Application.streamingAssetsPath, "mythic", "thomas"));
        var bank = bundle.LoadAsset<TextAsset>("Assets/FMODBanks/Thomas.bytes");
        RuntimeManager.LoadBank(bank);
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var dir = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
    }
}
