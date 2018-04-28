

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public	static 	List<Card>	Cards						= new List<Card>();

	[ReadOnly]
	public	int					Value						= 0;

	public	bool				InDeck						= true;

	private	bool				m_IsThrown					= false;
	public	bool				IsThrown
	{
		get { return m_IsThrown; }
		set { m_IsThrown = value; }
	}

	private	bool				m_Shown						= false;
	public	bool				Shown
	{
		get { return m_Shown; }
		set { SetState( value ); }
	}

	private	VirtualPlayer		m_DestinationPlayer			= null;
	public	VirtualPlayer		Owner
	{
		get { return m_DestinationPlayer; }
	}


	private	Camera				m_CameraRef					= null;
	private	GameObject			m_Foreground				= null;
	private	GameObject			m_Background				= null;
	private	Image				m_ImageBackground			= null;
	private	Image				m_ImageForeground			= null;

	private	float				m_CardSpeed					= 0f;
	private	Vector3				m_StartDragPosition			= Vector3.zero;
	private	Vector3				m_DragPosition				= Vector3.zero;
	private	Vector3				m_DirectionVector			= Vector3.zero;
	private	bool				m_ValidThrow				= false;
	


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_CameraRef = Camera.main;

		m_Foreground = transform.GetChild( 0 ).gameObject;
		m_Background = transform.GetChild( 1 ).gameObject;

		m_ImageBackground = m_Background.GetComponent<Image>();
		m_ImageForeground = m_Foreground.GetComponent<Image>();

		Cards.Add( this );

		this.SetState( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetState
	private	void	SetState( bool state )
	{
		m_Shown = state;

		m_Background.SetActive( !m_Shown );
		m_Foreground.SetActive( m_Shown );
	}


	//////////////////////////////////////////////////////////////////////////
	// IsVisible
	public	bool	IsVisible()
	{
		return UI.Instance.IsVisible( transform.GetChild( 0 ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		if ( GameManager.GameActive == false || m_IsThrown == false )
			return;

		transform.position += m_DirectionVector * Time.deltaTime * m_CardSpeed;

		if ( m_ValidThrow == true )
		{
			if ( m_DestinationPlayer.NeedCard() && ( transform.position - m_DestinationPlayer.transform.position ).sqrMagnitude < VirtualPlayer.PLAYER_RADIUS * VirtualPlayer.PLAYER_RADIUS )
			{
				m_DestinationPlayer.GetCard( this );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	public	void	OnReset()
	{
		this.SetState( false );
		m_StartDragPosition = m_DragPosition = m_DirectionVector = Vector3.zero;
		m_ValidThrow = false;
		m_CardSpeed = 0f;
		m_DestinationPlayer = null;
		InDeck = true;

		transform.SetParent( UI.Instance.Deck.transform );
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;

		m_ImageBackground.raycastTarget = true;
		m_ImageForeground.raycastTarget = true;

		m_StartDragPosition = transform.position;

		if ( GameManager.Instance.Deck.Contains( gameObject ) == false )
		{
			GameManager.Instance.Deck.Add( gameObject );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnPhaseChange
	public	void	OnPhaseChange()
	{
		if ( InDeck && m_IsThrown == false )
		{
			transform.localPosition = Vector3.zero;
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnPointerClick ( Interface )
	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if ( m_Shown == true )
			return;

		// Interaction has benn locked
		if ( GameManager.GameActive == false )
			return;

		// Only right button allowed
		if ( eventData.button != PointerEventData.InputButton.Right )
			return;

		// Only allowed during dealer phase
		if ( GameManager.Instance.CurrentPhase != GameManager.GAMEPHASE.DEALER )
			return;


		// Get a new card
		if ( Dealer.Instance.NeedCard() )
		{
			if ( InDeck )
			{
				Dealer.Instance.GetCard( this );
			}
			else
			{
				SetState( true );
			}
			GameManager.Instance.ChechGameStatus();
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnBeginDrag ( Interface )
	void IBeginDragHandler.OnBeginDrag( PointerEventData eventData )
	{
		// Allowed only on card in deck
		if ( InDeck == false )
			return;

		// Interaction has benn locked
		if ( GameManager.GameActive == false )
			return;
		
		// right button not alowed
		if ( eventData.button == PointerEventData.InputButton.Right )
			return;
		
		// not valid during dealer phase
		if ( GameManager.Instance.CurrentPhase == GameManager.GAMEPHASE.DEALER )
			return;


		m_StartDragPosition = transform.position;
		transform.SetAsLastSibling();
		transform.position = Input.mousePosition;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDrag ( Interface )
	void IDragHandler.OnDrag( PointerEventData eventData )
	{
		// Allowed only on card in deck
		if ( InDeck == false )
			return;

		// Interaction has benn locked
		if ( GameManager.GameActive == false )
			return;

		// right button not alowed
		if ( eventData.button == PointerEventData.InputButton.Right )
			return;

		// not valid during dealer phase
		if ( GameManager.Instance.CurrentPhase == GameManager.GAMEPHASE.DEALER )
			return;


		Vector3 direction = Vector3.ClampMagnitude( Input.mousePosition - m_StartDragPosition, GameManager.MAX_THROW_CHARGE );
		transform.position = m_StartDragPosition + direction;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEndDrag ( Interface )
	void IEndDragHandler.OnEndDrag( PointerEventData eventData )
	{
		// Allowed only on card in deck
		if ( InDeck == false )
			return;

		// Interaction has benn locked
		if ( GameManager.GameActive == false )
			return;

		// right button not alowed
		if ( eventData.button == PointerEventData.InputButton.Right )
			return;

		// not valid during dealer phase
		if ( GameManager.Instance.CurrentPhase == GameManager.GAMEPHASE.DEALER )
			return;


		float distance = Vector3.Distance( transform.position, m_StartDragPosition );
		if ( distance < 20 )
		{
			transform.position = m_StartDragPosition;
			return;
		}

		m_ImageBackground.raycastTarget = false;
		m_ImageForeground.raycastTarget = false;
		m_IsThrown = true;
		m_DirectionVector = ( transform.position - m_StartDragPosition ).normalized;
		m_CardSpeed = distance;

		for ( int i = 0; i < GameManager.MAX_PLAYERS; i++ )
		{
			VirtualPlayer player = VirtualPlayer.Players[ i ];

			// Giving card directly to a player
			if ( player.NeedCard() && ( Input.mousePosition - player.transform.position ).sqrMagnitude < VirtualPlayer.PLAYER_RADIUS * VirtualPlayer.PLAYER_RADIUS )
			{
				player.GetCard( this );
				InDeck = false;
				m_DestinationPlayer = player;
				GameManager.Instance.Deck.Remove( gameObject );
				return;
			}

			// Throwing card
			Vector3 directionDectToPlayer = ( player.transform.position -UI.Instance.Deck.transform.position );
			Vector3 directionDeckToCard   = ( transform.position - UI.Instance.Deck.transform.position );

			if ( Vector3.Angle( directionDectToPlayer, directionDeckToCard ) < 10f )
			{
				m_ValidThrow = true;
				m_DestinationPlayer = player;
				break;
			}
		}

		InDeck = false;
		transform.SetParent( transform.parent.parent );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		if ( Cards == null )
			return;

		Cards.Clear();
	}

}
