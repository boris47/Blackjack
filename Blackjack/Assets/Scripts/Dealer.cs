
using UnityEngine;

// The real player
public class Dealer : MonoBehaviour {

	public	static	Dealer		Instance			= null;

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

	private	int					m_CurentCardIndex	= 0;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		Instance = this;
		m_Cards = new Card[ GameManager.PLAYER_MAX_CARDS ];
	}


	//////////////////////////////////////////////////////////////////////////
	// NeedCard
	public	bool	NeedCard()
	{
		return m_CurentCardIndex < GameManager.PLAYER_MAX_CARDS;
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
			ace.Value = ( Points < 10 ) ? 11 : 1;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// GetCard
	public	void	GetCard( Card card )
	{
		if ( m_CurentCardIndex == GameManager.PLAYER_MAX_CARDS )
			return;

		if ( GameManager.Instance.Deck.Contains( card.gameObject ) )
			GameManager.Instance.Deck.Remove( card.gameObject );

		m_Cards[ m_CurentCardIndex ] = card;

		Transform snapPoint = transform.GetChild( m_CurentCardIndex );
		card.transform.SetParent( snapPoint );
		card.transform.localPosition = Vector3.zero;
		card.transform.localRotation = Quaternion.identity;

		card.InDeck = false;
		card.IsThrown = false;
		card.Shown = true;

		m_CurentCardIndex ++;

		if ( GameManager.GameActive == false )
			return;

		EvaluateCards();
		GameManager.Instance.ChechGameStatus();
	}


	//////////////////////////////////////////////////////////////////////////
	// GetCard
	public	void	OnReset()
	{
		m_CurentCardIndex = 0;
		System.Array.Clear( m_Cards, 0, ( int ) GameManager.PLAYER_MAX_CARDS );
	}


}
