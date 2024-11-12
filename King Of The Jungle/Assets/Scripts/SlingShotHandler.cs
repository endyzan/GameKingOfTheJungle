using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotHandler : MonoBehaviour
{
    [Header("Line Renderers")]
    [SerializeField] private LineRenderer _leftLineRenderer;
    [SerializeField] private LineRenderer _rightLineRenderer;

    [Header("Transform References")]
    [SerializeField] private Transform _leftStartRenderer;
    [SerializeField] private Transform _rightStartRenderer;
    [SerializeField] private Transform _centerPosition;
    [SerializeField] private Transform _idlePosition;

    [Header("SlingShot Stats")]
    [SerializeField] private float _maxDistance = 3.5f;
    [SerializeField] private float _shotForce = 5f;
    [SerializeField] private float _timeBetweenHarimauRespawns = 2f;

    [Header("Script")]
    [SerializeField] private SlingShotArea _slingShotArea;

    [Header("Hewan")]
    [SerializeField] private Harimau _HarimauPrefab;
    [SerializeField] private float _harimauPositionOffset = 2f;

    private Vector2 _slingShotLinePosition;
    private Vector2 _direction;
    private Vector2 _directionNormalized;

    private bool _clickedWithinArea;
    private bool _harimauOnSlingshot;

    private Harimau _spawnedHarimau;

    private void Awake()
    {
        _leftLineRenderer.enabled = false;
        _rightLineRenderer.enabled = false;

        SpawnHarimau();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _slingShotArea.IsWithinSlingShotArea())
        {
            _clickedWithinArea = true;
        }
        if (Mouse.current.leftButton.isPressed && _clickedWithinArea && _harimauOnSlingshot)
        {
            DrawSlingShot();
            PositionAndRotateHarimau();
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame && _harimauOnSlingshot)
        {
            if (GameManager.instance.HasEnoughShots())
            {
                _clickedWithinArea = false;
                _harimauOnSlingshot = false;

                _spawnedHarimau.LaunchHarimau(_direction, _shotForce);
                GameManager.instance.UseShot();
                SetLines(_centerPosition.position);

                StartCoroutine(SpawnHarimauAfterTime());
            }
        }
    }

    #region SlingShot Methods
    private void DrawSlingShot()
    {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _slingShotLinePosition = _centerPosition.position + Vector3.ClampMagnitude(touchPosition - _centerPosition.position, _maxDistance);
        SetLines(_slingShotLinePosition);
        _direction = (Vector2)_centerPosition.position - _slingShotLinePosition;
        _directionNormalized = _direction.normalized;
    }

    private void SetLines(Vector3 position)
    {
        if (!_leftLineRenderer.enabled && !_rightLineRenderer.enabled)
        {
            _leftLineRenderer.enabled = true;
            _rightLineRenderer.enabled = true;
        }
        _leftLineRenderer.SetPosition(0, position);
        _leftLineRenderer.SetPosition(1, _leftStartRenderer.position);
        _rightLineRenderer.SetPosition(0, position);
        _rightLineRenderer.SetPosition(1, _rightStartRenderer.position);
    }
    #endregion

    #region Harimau Methods
    private void SpawnHarimau()
    {
        SetLines(_idlePosition.position);
        Vector2 dir = (_centerPosition.position - _idlePosition.position).normalized;
        Vector2 spawnPosition = (Vector2)_idlePosition.position + dir * _harimauPositionOffset;

        _spawnedHarimau = Instantiate(_HarimauPrefab, _idlePosition.position, Quaternion.identity) as Harimau;

        _spawnedHarimau.transform.right = dir;
        _harimauOnSlingshot = true;
    }

    private void PositionAndRotateHarimau()
    {
        _spawnedHarimau.transform.position = _slingShotLinePosition;
        _spawnedHarimau.transform.right = _directionNormalized;
    }

    private IEnumerator SpawnHarimauAfterTime()
    {
        yield return new WaitForSeconds(_timeBetweenHarimauRespawns);
        SpawnHarimau();
    }
    #endregion
}