
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public	static	bool			GameActive						= false;

	public enum GAMEPHASE {
		PLAYERS, DEALER
	}

	private enum CardBackgroundColor {
		BLUE, RED
	}

	public	const	int				DECK_CAPACITY					= 52;
	public	const	int				MAX_PLAYERS						= 5;
	public	const	int				PLAYER_MAX_CARDS				= 5;

	public	const	float			MAX_THROW_CHARGE				= 100f;

	private	const	string			COLLECTIONPATH_CLOVERS			= "Collection_Clovers";
	private	const	string			COLLECTIONPATH_HEARTS			= "Collection_Hearts";
	private	const	string			COLLECTIONPATH_PIKES			= "Collection_Pikes";
	private	const	string			COLLECTIONPATH_TILES			= "Collection_Tiles";

	private	const	string			CARD_BLUE_PREFAB				= "Card_Blue";
	private	const	string			CARD_RED_PREFAB					= "Card_Red";
	private	const	string			VIRTUAL_PLAYER_PREFAB			= "VirtualPlayer";


	[SerializeField]
	private	float					m_HiddenCardShowTime			= 2f;

	private	static	GameManager		m_Instance						= null;
	public	static	GameManager		Instance
	{
		get { return m_Instance; }
	}


	private	List<GameObject>		m_Deck							= new List<GameObject>( DECK_CAPACITY );
	public	List<GameObject>		Deck
	{
		get { return m_Deck; }
	}

	private	GAMEPHASE				m_CurrentPhase					= GAMEPHASE.PLAYERS;
	public	GAMEPHASE				CurrentPhase
	{
		get { return m_CurrentPhase; }
	}

	
	private	GameObject				m_RedCardPrefab					= null;
	private	GameObject				m_BlueCardPrefab				= null;
	private	float					m_RoundStartTime				= 0f;
	private	int						m_HigherPlayerPoints			= 0;
	private	VirtualPlayer			m_WinnerPlayer					= null;



	//////////////////////////////////////////////////////////////////////////
	// Start
	private void Start()
	{
		// Save this instance in order to be visibile in global namespace
		m_Instance = this;

		// Load Sprite collections
		SpriteCollection	collectionClovers		= Resources.Load<SpriteCollection>( COLLECTIONPATH_CLOVERS );
		SpriteCollection	CollectionHearts		= Resources.Load<SpriteCollection>( COLLECTIONPATH_HEARTS );
		SpriteCollection	collectionPikes			= Resources.Load<SpriteCollection>( COLLECTIONPATH_PIKES );
		SpriteCollection	collectionTiles			= Resources.Load<SpriteCollection>( COLLECTIONPATH_TILES );

		// Load base card model
		m_RedCardPrefab		= Resources.Load<GameObject>( CARD_RED_PREFAB );
		m_BlueCardPrefab	= Resources.Load<GameObject>( CARD_BLUE_PREFAB );

		// Load player prefab
		GameObject virtualPlayerPrefab = Resources.Load<GameObject>( VIRTUAL_PLAYER_PREFAB );

		// Set deck capacity

		// Instantiate cards
		for ( int i = 0; i < collectionClovers.Collection.Length; i++ )		this.CreateCard( collectionClovers,	"Clovers_",	i + 1, CardBackgroundColor.BLUE );
		for ( int i = 0; i < CollectionHearts.Collection.Length; i++ )		this.CreateCard( CollectionHearts,	"Hearts_",	i + 1, CardBackgroundColor.RED  );
		for ( int i = 0; i < collectionPikes.Collection.Length; i++ )		this.CreateCard( collectionPikes,	"Pikes_",	i + 1, CardBackgroundColor.BLUE );
		for ( int i = 0; i < collectionTiles.Collection.Length; i++ )		this.CreateCard( collectionTiles,	"Tiles_",	i + 1, CardBackgroundColor.RED  );

		StartCoroutine( RoundStartCO() );
	}


	//////////////////////////////////////////////////////////////////////////
	// RoundStartCO ( Coroutine )
	private	IEnumerator	RoundStartCO()
	{
		UI.Instance.PrintStatus( "" );

		// Give two random cards to players
		for ( int i = 0; i < MAX_PLAYERS; i++ )
		{
			VirtualPlayer player = VirtualPlayer.Players[ i ];
			for ( int j = 0; j < 2; j++ )
			{
				Card randomCard = m_Deck[ Random.Range( 0, m_Deck.Count ) ].GetComponent<Card>();
				player.GetCard( randomCard );
			}
		}

		// Give two random cards to dealer, first is hidden
		Card randomCard1 = m_Deck[ Random.Range( 0, m_Deck.Count ) ].GetComponent<Card>();
		Dealer.Instance.GetCard( randomCard1 );
		Card randomCard2 = m_Deck[ Random.Range( 0, m_Deck.Count ) ].GetComponent<Card>();
		Dealer.Instance.GetCard( randomCard2 );

		// hide mouse
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		m_RoundStartTime = m_HiddenCardShowTime;
		while ( m_RoundStartTime > 0f )
		{
			m_RoundStartTime -= Time.deltaTime;
			yield return null;
		}

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		randomCard1.Shown = false;

		// Game setup is completed, players make their own decision
		VirtualPlayer.Players.ForEach( ( VirtualPlayer p ) => p.MakeDecision() );

		GameActive = true;

		UI.Instance.PrintStatus( "Players Turn" );

		this.ChechGameStatus();
	}


	//////////////////////////////////////////////////////////////////////////
	// CreateCard
	private	void	CreateCard( SpriteCollection collection, string prefix, int cardValue, CardBackgroundColor background )
	{
		// Choose whitch card to instantiate
		GameObject objToInstantiate = ( background == CardBackgroundColor.BLUE ) ? m_BlueCardPrefab : m_RedCardPrefab;

		// Instantiate object
		GameObject newCard = Object.Instantiate( objToInstantiate, UI.Instance.Deck );

		// Change to a logic readable name
		newCard.name = ( prefix + cardValue.ToString() );

		// Get foreground and set it
		Sprite foreground = collection.Collection[ cardValue - 1 ];
		Image renderer = newCard.transform.GetChild( 0 ).GetComponent<Image>();
		renderer.sprite = foreground;

		// Set card value
		Card cardComponent = newCard.GetComponent<Card>();
		cardComponent.Value = Mathf.Clamp( cardValue, 1, 10 );

		// Add it into deck
		m_Deck.Add( newCard );
	}


	//////////////////////////////////////////////////////////////////////////
	// ChechGameStatus
	public	void	ChechGameStatus()
	{
		if ( m_CurrentPhase == GAMEPHASE.PLAYERS )
		{
			foreach( VirtualPlayer player in VirtualPlayer.Players )
			{
				if ( player.Done == false )
					return;
			}

			// Reached this point is the turn of dealer, show hidden card
			m_CurrentPhase = GAMEPHASE.DEALER;

			// Reset card is currently dragging
			Card.Cards.ForEach( ( Card c ) => c.OnPhaseChange() );
			UI.Instance.PrintStatus( "Dealer Turn" );
			Dealer.Instance.Cards[0].Shown = true;
			GameManager.Instance.ChechGameStatus();
		}
		else	// check dealer state
		{
			int dealerPoints = Dealer.Instance.Points;

			if ( dealerPoints > 21 )
			{
				UI.Instance.OnGameEnd( "Dealer bust" );
				return;
			}

			if ( dealerPoints < 17 )
				return;

			string winners = "";
			foreach( VirtualPlayer player in VirtualPlayer.Players )
			{
				if ( player.CurrentDecision == VirtualPlayer.PlayerDecision.BUST )
					continue;

				if ( player.Points > dealerPoints )
					winners += player.name + " ";
			}

			if ( winners.Length == 0 )
				winners = "dealer";

			UI.Instance.OnGameEnd( "Winners:\n" + winners );

			GameManager.GameActive = false;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReet
	public	void	NewRound()
	{
		m_WinnerPlayer = null;
		m_HigherPlayerPoints = 0;
		GameActive = false;

		// Reset cards
		Card.Cards.ForEach( ( Card c ) => c.OnReset() );

		// Reset players
		VirtualPlayer.Players.ForEach( ( VirtualPlayer p ) => p.OnReset() );

		Dealer.Instance.OnReset();

		m_CurrentPhase = GAMEPHASE.PLAYERS;

		StartCoroutine( RoundStartCO() );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

	}

}
