Public Class MPC_Enum
    ''' <summary>
    ''' Elenco delle costanti relative ai Comandi inviati da MPC
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum CMD_RECEIVED As Integer
        ''' <summary>
        ''' Comando CONNESSO
        ''' </summary>
        ''' <remarks>Connessione stabilita tra MPC e l'oggetto di gestione
        ''' e viceversa</remarks>
        CMD_CONNECT = &H50000000
        ''' <summary>
        ''' Comando STATO
        ''' </summary>
        ''' <remarks>Indica che il messaggio specifica lo stato attuale di MPC.
        ''' Fare riferimento all'elenco LOADSTATE per gli stati specificati.</remarks>
        CMD_STATE = &H50000001
        ''' <summary>
        ''' Comando MODO PLAY
        ''' </summary>
        ''' <remarks>Specifica lo stato di MPC in riproduzione.
        ''' Fare riferimento all'elenco PLAYSTATE per gli stati specificati</remarks>
        CMD_PLAYMODE = &H50000002
        ''' <summary>
        ''' Comando NOWPLAYING
        ''' </summary>
        ''' <remarks>Specifica che vengono restituite informazioni nel messaggio nel
        ''' formato: "titolo|autore|descrizione|nomefile|durata". Il separatore è il
        ''' carattere | (pipe). Ove sia presente nelle stringhe il carattere "|", verrà
        ''' sostituito con "\|" per distinguere dal separatore. </remarks>
        CMD_NOWPLAYING = &H50000003
        ''' <summary>
        ''' Comando LISTA DELLE TRACCE SOTTOTITOLO
        ''' </summary>
        ''' <remarks>Specifica che il messaggio contiene la lista delle tracce
        ''' separate dal carattere "|" pipe (per distinguere il separatore da un 
        ''' carattere della stringa, nella stringa "|" verrà sostituito con "\|").
        ''' Formato messaggio: "track1|track2|...|trackN". L'ultima traccia è quella
        ''' attiva. Se viene restituito "-1" non è presente nessuna traccia. Se
        ''' viene restituito "-2" non è caricato nessun file sottotitolo. </remarks>
        CMD_LISTSUBTITLETRACKS = &H50000004
        ''' <summary>
        ''' Comando LISTA DELLE TRACCE AUDIO
        ''' </summary>
        ''' <remarks>Specifica che il messaggio contiene la lista delle tracce
        ''' separate dal carattere "|" pipe (per distinguere il separatore da un 
        ''' carattere della stringa, nella stringa "|" verrà sostituito con "\|").
        ''' Formato messaggio: "track1|track2|...|trackN". L'ultima traccia è quella
        ''' attiva. Se viene restituito "-1" non è presente nessuna traccia. Se
        ''' viene restituito "-2" non è caricato nessun file audio. </remarks>
        CMD_LISTAUDIOTRACKS = &H50000005
        ''' <summary>
        ''' Comando POSIZIONE CORRENTE
        ''' </summary>
        ''' <remarks>Specifica che il messaggio contiene la posizione in secondi,
        ''' sullo stream corrente.</remarks>
        CMD_CURRENTPOSITION = &H50000007
        ''' <summary>
        ''' Comando NOTIFICA SALTO
        ''' </summary>
        ''' <remarks>Specifica che è stato effettuato un salto temporale e che il 
        ''' messaggio contiene la posizione attuale</remarks>
        CMD_NOTIFYSEEK = &H50000008
        ''' <summary>
        ''' Comando NOTIFICA LA FINE DELLO STREAM
        ''' </summary>
        ''' <remarks>Specifica che si è raggiunta la fine dello stream. Il messaggio
        ''' non contiene dati</remarks>
        CMD_NOTIFYENDOFSTREAM = &H50000009
        ''' <summary>
        ''' Comando LISTA DI RIPRODUZIONE
        ''' </summary>
        ''' <remarks>Specifica che il messaggio contiene la lista delle tracce
        ''' separate dal carattere "|" pipe (per distinguere il separatore da un 
        ''' carattere della stringa, nella stringa "|" verrà sostituito con "\|").
        ''' Formato messaggio: "track1|track2|...|trackN". L'ultima traccia è quella
        ''' attiva. Se viene restituito "-1" non è presente nessuna traccia.</remarks>
        CMD_PLAYLIST = &H50000006
    End Enum
    ''' <summary>
    ''' Elenco delle costanti relative ai Comandi riconosciuti da MPC
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum CMD_SEND As Integer
        WM_SIZE = &H5
        ''' <summary>
        ''' Comando APRI FILE
        ''' </summary>
        ''' <remarks>Il messaggio deve indicare nome file e percorso</remarks>
        CMD_OPENFILE = &HA0000000
        ''' <summary>
        ''' Comando STOP
        ''' </summary>
        ''' <remarks></remarks>
        CMD_STOP = &HA0000001
        ''' <summary>
        ''' Comando CHIUDI FILE
        ''' </summary>
        ''' <remarks></remarks>
        CMD_CLOSEFILE = &HA0000002
        ''' <summary>
        ''' Comando PLAY/PAUSA
        ''' </summary>
        ''' <remarks></remarks>
        CMD_PLAYPAUSE = &HA0000003
        ''' <summary>
        ''' Comando AGGIUNGI FILE ALLA PLAYLIST
        ''' </summary>
        ''' <remarks>Il messaggio deve contenere nome file e percorso da aggiungere 
        ''' in playlist</remarks>
        CMD_ADDTOPLAYLIST = &HA0001000
        ''' <summary>
        ''' Comando SVUOTA PLAYLIST
        ''' </summary>
        ''' <remarks></remarks>
        CMD_CLEARPLAYLIST = &HA0001001
        ''' <summary>
        ''' Comando PLAY PLAYLIST
        ''' </summary>
        ''' <remarks></remarks>
        CMD_STARTPLAYLIST = &HA0001002
        ''' <summary>
        ''' Comando RIMUOVI FILE DALLA PLAYLIST
        ''' </summary>
        ''' <remarks>Il messaggio deve contenere l'indice del file da rimuovere.
        ''' COMANDO PREVISTO MA NON IMPLEMENTATO NELLE API MPC</remarks>
        CMD_REMOVEFROMPLAYLIST = &HA0001003 ' >> TODO
        ''' <summary>
        ''' Comando IMPOSTA POSIZIONE
        ''' </summary>
        ''' <remarks>Il messaggio deve contenere la nuova posizione in secondi
        ''' sullo stream corrente</remarks>
        CMD_SETPOSITION = &HA0002000
        ''' <summary>
        ''' Comando IMPOSTA RITARDO AUDIO
        ''' </summary>
        ''' <remarks>Il messaggio deve contenere il ritardo in millisecondi del
        ''' flusso audio</remarks>
        CMD_SETAUDIODELAY = &HA0002001
        ''' <summary>
        ''' Comando IMPOSTA RITARDO SOTTOTITOLI
        ''' </summary>
        ''' <remarks>Il messaggio deve specificare il ritardo in millisecondi dei
        ''' sottotitoli</remarks>
        CMD_SETSUBTITLEDELAY = &HA0002002
        ''' <summary>
        ''' Comando IMPOSTA FILE CORRENTE IN PLAYLIST
        ''' </summary>
        ''' <remarks>Il messaggio deve contenere l'indice del file da rendere attivo.
        ''' ATTUALMENTE NON FUNZIONANTE</remarks>
        CMD_SETINDEXPLAYLIST = &HA0002003 ' >> DOESN'T WORK
        ''' <summary>
        ''' Comando IMPOSTA TRACCIA AUDIO
        ''' </summary>
        ''' <remarks>Il messaggio deve specificare l'indice della traccia audio da
        ''' selezionare</remarks>
        CMD_SETAUDIOTRACK = &HA0002004
        ''' <summary>
        ''' Comando IMPOSTA TRACCIA SOTTOTITOLO
        ''' </summary>
        ''' <remarks>Il messaggio deve specificare l'indice della traccia sottotitolo
        ''' da selezionare</remarks>
        CMD_SETSUBTITLETRACK = &HA0002005
        ''' <summary>
        ''' Comando RESTITUISCI TRACCE SOTTOTITOLO
        ''' </summary>
        ''' <remarks></remarks>
        CMD_GETSUBTITLETRACKS = &HA0003000
        ''' <summary>
        ''' Comando RESTITUISCI POSIZIONE CORRENTE
        ''' </summary>
        ''' <remarks></remarks>
        CMD_GETCURRENTPOSITION = &HA0003004
        ''' <summary>
        ''' Comando SALTA DI N SECONDI
        ''' </summary>
        ''' <remarks>Il messaggio deve specificare un valore in secondi da sommare
        ''' alla posizione corrente per effettuare il salto. Accetta valori positivi
        ''' per andare avanti e negativi per andare indietro.</remarks>
        CMD_JUMPOFNSECONDS = &HA0003005
        ''' <summary>
        ''' Comando RESTITUISCI TRACCE AUDIO
        ''' </summary>
        ''' <remarks></remarks>
        CMD_GETAUDIOTRACKS = &HA0003001
        ''' <summary>
        ''' Comando RESTITUISCI INFO SULLO STREAM CORRENTE
        ''' </summary>
        ''' <remarks>ATTUALMENTE NON FUNZIONANTE</remarks>
        CMD_GETNOWPLAYING = &HA0003002
        ''' <summary>
        ''' Comando RESTITUISCI LA LISTA DEGLI ELEMENTI IN PLAYLIST
        ''' </summary>
        ''' <remarks></remarks>
        CMD_GETPLAYLIST = &HA0003003
        ''' <summary>
        ''' Comando SWITCH SCHERMO INTERO
        ''' </summary>
        ''' <remarks></remarks>
        CMD_TOGGLEFULLSCREEN = &HA0004000
        ''' <summary>
        ''' Comando SALTO BREVE AVANTI
        ''' </summary>
        ''' <remarks></remarks>
        CMD_JUMPFORWARDMED = &HA0004001
        ''' <summary>
        ''' Comando SALTO BREVE INDIETRO
        ''' </summary>
        ''' <remarks></remarks>
        CMD_JUMPBACKWARDMED = &HA0004002
        ''' <summary>
        ''' Comando AUMENTA VOLUME
        ''' </summary>
        ''' <remarks></remarks>
        CMD_INCREASEVOLUME = &HA0004003
        ''' <summary>
        ''' Comando DIMINUISCI VOLUME
        ''' </summary>
        ''' <remarks></remarks>
        CMD_DECREASEVOLUME = &HA0004004
        ''' <summary>
        ''' Comando SWITCH SHADER
        ''' </summary>
        ''' <remarks></remarks>
        CMD_SHADER_TOGGLE = &HA0004005
        ''' <summary>
        ''' Comando CHIUDI MPC
        ''' </summary>
        ''' <remarks></remarks>
        CMD_CLOSEAPP = &HA0004006
        ''' <summary>
        ''' Comando MOSTRA MESSAGGIO OSD
        ''' </summary>
        ''' <remarks>Il messaggio deve specificare un gruppo di tre parametri:
        ''' Posizione del messaggio (fare riferimento all'elenco OSD_POSITION);
        ''' Durata del messaggio (in millisecondi);
        ''' Messaggio contenuto in un array di CHAR (max 128 char)</remarks>
        CMD_OSDSHOWMESSAGE = &HA0005000
    End Enum
    ''' <summary>
    ''' Elenco delle costanti relative al comando CMD_RECEIVED.CMD_STATE
    ''' </summary>
    ''' <remarks>Specificate nel messaggio. Indicano lo stato di MPC</remarks>
    Public Enum LOADSTATE As Integer
        ''' <summary>
        ''' Stato CHIUSO
        ''' </summary>
        ''' <remarks></remarks>
        MLS_CLOSED = 0
        ''' <summary>
        ''' Stato IN CARICAMENTO
        ''' </summary>
        ''' <remarks></remarks>
        MLS_LOADING = 1
        ''' <summary>
        ''' Stato CARICATO
        ''' </summary>
        ''' <remarks></remarks>
        MLS_LOADED = 2
        ''' <summary>
        ''' Stato IN CHIUSURA
        ''' </summary>
        ''' <remarks></remarks>
        MLS_CLOSING = 3
    End Enum
    ''' <summary>
    ''' Elenco delle costanti relative al comando CMD_RECEIVED.CMD_PLAYMODE
    ''' </summary>
    ''' <remarks>Specificate nel messaggio. Indicano lo stato in riproduzione</remarks>
    Public Enum PLAYSTATE As Integer
        ''' <summary>
        ''' Stato PLAY
        ''' </summary>
        ''' <remarks></remarks>
        PS_PLAY = 0
        ''' <summary>
        ''' Stato PAUSA
        ''' </summary>
        ''' <remarks></remarks>
        PS_PAUSE = 1
        ''' <summary>
        ''' Stato STOP
        ''' </summary>
        ''' <remarks></remarks>
        PS_STOP = 2
        ''' <summary>
        ''' Stato NON USATO
        ''' </summary>
        ''' <remarks></remarks>
        PS_UNUSED = 3
    End Enum
    ''' <summary>
    ''' Elenco delle costanti relative alla posizione del messaggio OSD per
    ''' il comando CMD_SEND.Cmd_OSDSHOWMESSAGE
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum OSD_POSITION As Integer
        ''' <summary>
        ''' Posizione NESSUN MESSAGGIO
        ''' </summary>
        ''' <remarks></remarks>
        OSD_NOMESSAGE = 0
        ''' <summary>
        ''' Posizione IN ALTO A SINISTRA
        ''' </summary>
        ''' <remarks></remarks>
        OSD_TOPLEFT = 1
        ''' <summary>
        ''' Posizione IN ALTO A DESTRA
        ''' </summary>
        ''' <remarks></remarks>
        OSD_TOPRIGHT = 2
    End Enum
End Class
