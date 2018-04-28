
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {
	
	private	static	UI		m_Instance			= null;
	public	static	UI		Instance
	{
		get { return m_Instance; }
	}


	private	Canvas			m_GameCanvas		= null;
	public	Canvas			GameCanvas
	{
		get { return m_GameCanvas; }
	}
	
	private	Transform		m_Deck				= null;
	public	Transform		Deck
	{
		get { return m_Deck; }
	}


	private	Transform[]		m_Players			= null;
	public	Transform[]		Players
	{
		get { return m_Players; }
	}


	private	Text			m_StatusText		= null;
	private	GameObject		m_RestartButton		= null;
	private	Text			m_StatusBarText		= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_Instance = this;

		m_GameCanvas	= this.GetComponent<Canvas>();
		m_StatusText	= transform.Find( "GameStatus" ).GetComponent<Text>();
		m_StatusBarText	= transform.Find( "StatusBar" ).GetComponent<Text>();

		m_StatusText.gameObject.SetActive( false );
		m_RestartButton	= transform.Find( "Button_Restart" ).gameObject;
		m_RestartButton.SetActive( false );

		m_Deck			= transform.Find( "Deck" );

		m_Players			= new Transform[ GameManager.MAX_PLAYERS ];
		for ( int i = 0; i < GameManager.MAX_PLAYERS; i++ )
		{
			Transform playerTransform = transform.Find( "Player" + ( i + 1 ).ToString() );
			m_Players[ i ] = playerTransform;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// PrintStatus
	public	void	PrintStatus( string output )
	{
		m_StatusBarText.text = output;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnShuffle
	public	void	OnShuffle()
	{
		int deckCount = GameManager.Instance.Deck.Count;

		foreach ( Card card in Card.Cards )
		{
			if ( card.InDeck == true )
			{
				card.transform.SetSiblingIndex( Random.Range( 0, deckCount ) );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnRecover
	public	void	OnRecover()
	{
		foreach( Card card in Card.Cards )  // lol
		{
			if ( card.InDeck == false && card.Owner == null && card.IsVisible() == false )
			{
				card.OnReset();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnNewRound
	public	void	OnNewRound()
	{
		m_StatusText.gameObject.SetActive( false );
		m_RestartButton.SetActive( false );

		GameManager.Instance.NewRound();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnGameEnd
	public	void	OnGameEnd( string output )
	{
		GameManager.GameActive = false;
		m_StatusText.text = output;
		m_StatusText.gameObject.SetActive( true );
		m_RestartButton.SetActive( true );
	}





	// https://forum.unity.com/threads/cull-ui-if-recttransform-goes-outside-viewport-screen-canvas.470226/
	//////////////////////////////////////////////////////////////////////////
	// IsVisible
	public bool IsVisible( Transform otherTransform )
	{
		RectTransform thisRectTransfomr = transform as RectTransform;
		RectTransform othtRectTransform = otherTransform as RectTransform;

		Rect thisRect	= GetScreenSpaceRect( thisRectTransfomr );
		Rect otherRect	= GetScreenSpaceRect( othtRectTransform );

		return thisRect.Overlaps( otherRect );
    }



	//rect transform into coordinates expressed as seen on the screen (in pixels)
	//takes into account RectTrasform pivots
	// based on answer by Tobias-Pott
	// http://answers.unity3d.com/questions/1013011/convert-recttransform-rect-to-screen-space.html
	//////////////////////////////////////////////////////////////////////////
	// GetScreenSpaceRect
	private Rect GetScreenSpaceRect( RectTransform rectTransform )
	{
        Vector2 size = Vector2.Scale( rectTransform.rect.size, rectTransform.lossyScale );
        Rect rect = new Rect( rectTransform.position.x, Screen.height - rectTransform.position.y, size.x, size.y );
        rect.x -= ( rectTransform.pivot.x * size.x );
        rect.y -= ( ( 1.0f - rectTransform.pivot.y ) * size.y );

        return rect;
    }


}
