using UnityEngine;

public class SawController : MonoBehaviour
{
    public float RotationSpeed;
    public float Range;
    private bool _changedColorOnce = false; //we want the Saw to only be able to change color once
    private SpriteRenderer _spriteRenderer;
    private Transform _playerTransform;


    void Start()
    {
        _playerTransform = GameObject.Find("Runner").transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }



    void FixedUpdate()
    {
        transform.Rotate(0, 0, RotationSpeed);


        if (_changedColorOnce == false && _playerTransform.position.x >= gameObject.transform.position.x - Range)
        {
            _changedColorOnce = true; //do not change again

            if (StateManager.CurrentState.PlayerState.PlayerColor== PlayerColor.Cyan)// .IsCyan)
            {
                _spriteRenderer.color = Color.green;
                gameObject.tag = "GreenSaw";
            }
            else
            {
                _spriteRenderer.color = Color.cyan;
                gameObject.tag = "CyanSaw";
            }
        }
    }



}