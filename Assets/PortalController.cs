using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PortalController : MonoBehaviour {


public    SpriteRenderer _frontEffectRenderer;
    public SpriteRenderer _backEffectRenderer;

	// Use this for initialization
	void Start () {

        _frontEffectRenderer.color = _backEffectRenderer.color = Color.green;

    }
	



}
