
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualPlayer : MonoBehaviour {
	
	public	static	List<VirtualPlayer>		Players	= new List<VirtualPlayer>();

	public	const	float		PLAYER_RADIUS		= 300f;

	public enum PlayerDecision {
		/// <summary>
		/// NONE
		/// </summary>
		NONE,
		/// <summary>
		/// BLACKJACK
		/// </summary>
		BLACKJACK,
		/// <summary>
		/// Another card
		/// </summary>
		CARD,
		/// <summary>
		/// fail
		/// </summary>
		BUST,
		/// <summary>
		/// it's fine
		/// </summary>
		STOP
	}

	private	PlayerDecision		m_CurrentDecision	= PlayerDecision.NONE;
	public	PlayerDecision		CurrentDecision
	{
		get { return m_CurrentDecision; }
	}

	private	Card[]				m_Cards				= null;
	public	Card[]				Cards
	{
		get { return m_Cards; }
	}

	public	int	Points
	{
		get
		{
			int currentPoints = 0;
			System.Array.ForEach( m_Cards, ( Card c ) => {  if ( c ) currentPoints += c.Value; } );
			return currentPoints;
		}
	}

	private	bool				m_Done				= false;
	public	bool				Done
	{
		get { return m_Done; }
	}

	public	bool				IsValid
	{
		get { return m_CurrentDecision == PlayerDecision.BLACKJACK || m_CurrentDecision == PlayerDecision.CARD || m_CurrentDecision == PlayerDecision.STOP; }
	}

	private	int					m_CurentCardCount	= 0;
	private	bool				m_IsActive			= true;
	private	Text				m_DecisionText		= null;
	private	float				m_Courage			= 1f;




	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_DecisionText = transform.Find( "Decision" ).GetComponent<Text>();

		m_Cards = new Card[ GameManager.PLAYER_MAX_CARDS ];

		m_Courage = Random.value;

		Players.Add( this );
	}


	//////////////////////////////////////////////////////////////////////////
	// NeedCard
	public	bool	NeedCard()
	{
		return
			// Wants a card
			m_IsActive == true &&
			m_CurrentDecision == PlayerDecision.CARD &&
			m_CurentCardCount < GameManager.PLAYER_MAX_CARDS;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	private	void	EvaluateCards()
	{
		// total points ammout is over 21, bust decision
		if ( Points > 21 )
			return;

		// search for an ace
		Card ace = System.Array.Find( m_Cards, ( Card c ) => ( c ) && c.Value == 1 );

		// If ace found evaluate its value for "soft" or "hard" hand
		if ( ace != null )
		{
			ace.Value = 11;
			ace.Value = ( Points > 21 ) ? 1 : 11;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// MakeDecision
	public	void	MakeDecision()
	{
		if ( m_IsActive == false )
			return;

		EvaluateCards();

		int currentPoints = Points;

		// Bust
		if ( currentPoints > 21 )
		{
			m_CurrentDecision = PlayerDecision.BUST;
			m_DecisionText.text = "Bust";
			m_Done = true;
			return;
		}

		// Blackjack
		if ( m_CurentCardCount == 2 && currentPoints == 21 )
		{
			m_CurrentDecision = PlayerDecision.BLACKJACK;
			m_DecisionText.text = "Blackjack";
			m_Done = true;
			return;
		}

		// Stop ( what a lucky )
		if ( currentPoints == 21 )
		{
			m_CurrentDecision = PlayerDecision.STOP;
			m_DecisionText.text = "Stop";
			m_Done = true;
			return;
		}


		float randomValue = Random.value * m_Courage;

		// Card
		if ( currentPoints < 20 && randomValue > 0.4f )
		{
			m_CurrentDecision = PlayerDecision.CARD;
			m_DecisionText.text = "Card";
			
		}
		else	// Stop
		{
			m_CurrentDecision = PlayerDecision.STOP;
			m_DecisionText.text = "Stop";
			m_Done = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// GetCard
	public	void	GetCard( Card card )
	{
		if ( m_IsActive == false || m_CurentCardCount == GameManager.PLAYER_MAX_CARDS )
			return;

		if ( GameManager.Instance.Deck.Contains( card.gameObject ) )
			GameManager.Instance.Deck.Remove( card.gameObject );

		m_Cards[ m_CurentCardCount ] = card;

		Transform snapPoint = transform.GetChild( m_CurentCardCount );
		card.transform.SetParent( snapPoint );
		card.transform.localPosition = Vector3.zero;
		card.transform.localRotation = Quaternion.identity;

		card.InDeck = false;
		card.IsThrown = false;
		card.Shown = true;

		m_CurentCardCount ++;

		if ( GameManager.GameActive == false )
			return;

		this.MakeDecision();
		GameManager.Instance.ChechGameStatus();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	public void OnReset()
	{
		m_DecisionText.text = "";
		m_CurentCardCount = 0;
		m_Done = false;
		System.Array.Clear( m_Cards, 0, ( int ) GameManager.PLAYER_MAX_CARDS );
	}


	private void OnDestroy()
	{
		if ( Players == null )
			return;

		Players.Clear();
	}

}
