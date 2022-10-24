using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(BoxCollider))]
public class ElevatorSimple : MonoBehaviour
{
    private const float DEFAULT_TRAVEL_TIME = 7f;

    [SerializeField]
    [Tooltip("The amount of time it takes to travel across the escalator in seconds.")]
    private float _travelTime = DEFAULT_TRAVEL_TIME;

    [SerializeField]
    [Tooltip("Should the escalator move downwards?")]
    private bool _goesDown;

    private float _length;
    private HashSet<CharacterController> _characters = new HashSet<CharacterController>();

    private void Start()
    {
        var collider = GetComponent<BoxCollider>();
        _length = Mathf.Max(Mathf.Max(collider.size.x * transform.lossyScale.x, collider.size.y * transform.lossyScale.y), collider.size.z * transform.lossyScale.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        var characterController = other.GetComponent<CharacterController>();
        if (!_characters.Contains(characterController))
        {
            _characters.Add(characterController);

            var firstPersonController = other.GetComponent<FirstPersonController>();
            if (firstPersonController != null)
            {
                firstPersonController.EnableHeadBob(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var characterController = other.GetComponent<CharacterController>();
        if (_characters.Contains(characterController))
        {
            _characters.Remove(characterController);

            var firstPersonController = other.GetComponent<FirstPersonController>();
            if (firstPersonController != null)
            {
                firstPersonController.EnableHeadBob(true);
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (var characterController in _characters)
        {
            var v = _goesDown ? Vector3.left : Vector3.right;
            var t = _travelTime != 0f ? _travelTime : DEFAULT_TRAVEL_TIME;
            var move = (transform.rotation * v * Time.fixedDeltaTime * (_length / t));
            characterController.Move(move);
        }
    }
}
