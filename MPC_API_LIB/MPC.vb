Imports MPC_API_LIB.MPC_Enum
Imports System.Threading
Imports System.Diagnostics
Imports System.Threading.Tasks

Public Class MPC
    Private Const CommandTimeout As Integer = 3000
    Private Declare Function MoveWindow Lib "user32" (ByVal hwnd As IntPtr, ByVal x As Integer, ByVal y As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal bRepaint As Boolean) As Boolean
    Private Declare Function ShowWindow Lib "user32" (ByVal hwnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    Private Const SW_SHOWMAXIMIZED As Integer = 3
    Private Delegate Sub SendCommandAsyncCaller(ByVal cmd As MPC_Enum.CMD_SEND, ByVal param As String)

    Public Sub ResizeWindow(posX As Integer, posY As Integer, width As Integer, height As Integer)
        MoveWindow(Comm.HandleTo, posX, posY, width, height, True)
    End Sub

    Public Sub MaximizeWindow()
        ShowWindow(Comm.HandleTo, SW_SHOWMAXIMIZED)
    End Sub

    ''' <summary>
    ''' Evento generato quando la connessione a MPC va a buon fine
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Generato dopo l'esecuzione della routine Run</remarks>
    Public Event MPC_Connected(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato al completamento della chiusura di un file
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato dopo l'evento MPC_Closing a seguito delle routine
    ''' StopAndClose, CloseMPC e al termine della riproduzione se prevista la 
    ''' chiusura del file
    ''' </remarks>
    Public Event MPC_Closed(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato dopo il comando di chiusura di un file (durante la chiusura)
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato a seguito della richiesta di chiusura delle routine
    ''' StopAndClose, CloseMPC e al termine della riproduzione se prevista la 
    ''' chiusura del file
    ''' </remarks>
    Public Event MPC_Closing(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato al completamento del caricamento di un file (per la riproduzione)
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato a seguito della richiesta di apertura delle routine
    ''' OpenFile e StartPlaylist, ad apertura completata e prima della riproduzione
    ''' </remarks>
    Public Event MPC_Loaded(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato durante il caricamento di un file (per la riproduzione)
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato a seguito della richiesta di apertura delle routine
    ''' OpenFile e StartPlaylist, durante il caricamento del file prima della riproduzione
    ''' </remarks>
    Public Event MPC_Loading(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento di stato sconosciuto
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato a seguito di richieste di stato non 
    ''' documentato
    ''' </remarks>
    Public Event MPC_UnknownState(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato all'avvio riproduzione (play)
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato a seguito dell'avvio riproduzione anche dopo lo stato
    ''' di pausa
    ''' </remarks>
    Public Event MPC_Play(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato alla messa in pausa della riproduzione
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks></remarks>
    Public Event MPC_Pause(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato al termine della riproduzione
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato a seguito del termine della riproduzione 
    ''' per raggiungimento della fine dello stream o per un comando di stop
    ''' </remarks>
    Public Event MPC_Stop(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento "non usato" che coinvolge lo stato durante la riproduzione
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento previsto dalle API MPC ma non utilizzato
    ''' </remarks>
    Public Event MPC_Unused(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato in seguito ad uno stato sconosciuto che coinvolge la riproduzione
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento sconosciuto generato in ambito di riproduzione
    ''' </remarks>
    Public Event MPC_UnknownPlayState(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato per restituire Titolo, Autore, Descrizione, Nome e percorso file
    ''' e Durata dello stream attualmente caricato e/o in riproduzione
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato a seguito di richiesta info sullo stream caricato
    ''' e in seguito al caricamento dello stream (prima dell'evento di PLAY)
    ''' </remarks>
    Public Event MPC_NowPlaying(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato alla richiesta della lista delle tracce sottotitolo
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato solo su richiesta della lista delle tracce sottotitolo
    ''' GetSubtitleTracks
    ''' </remarks>
    Public Event MPC_ListSubtitleTracks(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato alla richiesta della lista delle tracce audio
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato solo in seguito alla richiesta delle tracce audio
    ''' GetAudioTracks
    ''' </remarks>
    Public Event MPC_ListAudioTracks(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato in seguito a richiesta della corrente posizione sullo stream caricato
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato a seguito della richiesta esplicita GetCurrentPosition
    ''' o in seguito a salti nella riproduzione con JumpBackward o JumpForward
    ''' e con intervento manuale su MPC
    ''' </remarks>
    Public Event MPC_CurrentPosition(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato al salto in riproduzione con intervento diretto su MPC
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks></remarks>
    Public Event MPC_NotifySeek(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato alla conclusione della riproduzione per raggiungimento della
    ''' fine dello stream
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato al raggiungimento della fine dello stream
    ''' </remarks>
    Public Event MPC_NotifyEndOfStream(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generato in risposta ad una richiesta di recupero della playlist
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generato in seguito a chiamata della routine GetPlaylist
    ''' </remarks>
    Public Event MPC_Playlist(ByVal msg As MPC.MSGIN)
    ''' <summary>
    ''' Evento generico generato per un qualsiasi messaggio ricevuto da MPC
    ''' </summary>
    ''' <param name="msg">Messaggio restituito in background via WM_COPYDATA</param>
    ''' <remarks>Evento generico di ricezione di un messaggio in background via 
    ''' WM_COPYDATA da MPC
    ''' </remarks>
    Public Event MPC_GENERIC(ByVal msg As MPC.MSGIN)

    Public Event MPC_Running()
    Public Event MPC_SendingCommand(ByVal cmd As MPC_Enum.CMD_SEND, ByVal param As String)

    Private Percorso As String
    Private AppName As String = "mpc-hc.exe"
    Private Hndl As IntPtr
    Private nowplay As MPCToHost.INFOPLAYING
    Private WithEvents Comm As New CommWnd
    Private msgR As MSGIN
    Public MpcProcess As Process
    ''' <summary>
    ''' Struttura che individua una coppia Comando-Messaggio ricevuti via WM_COPYDATA
    ''' in background
    ''' </summary>
    ''' <remarks>Per il parametro Command è possibile fare riferimento a 
    ''' MPC_Enum.CMD_RECEIVED di tipo Integer
    ''' </remarks>
    Public Structure MSGIN
        ''' <summary>
        ''' Parametro relativo al comando da inviare in un canale WM_COPYDATA
        ''' </summary>
        ''' <remarks></remarks>
        Dim Message As String
        ''' <summary>
        ''' Comando da inviare in un canale WM_COPYDATA
        ''' </summary>
        ''' <remarks>Fare riferimento a MPC_Enum.CMD_RECEIVED per l'elenco dei 
        ''' comandi riconosciuti da MPC</remarks>
        Dim Command As MPC_Enum.CMD_RECEIVED
    End Structure
    ''' <summary>
    ''' Struttura che individua i parametri per l'invio di informazioni OSD su MPC
    ''' </summary>
    ''' <remarks>Fare riferimento a MPC_Enum.OSD_POSITION per determinare la posizione
    ''' del messaggio OSD su schermo
    ''' </remarks>
    Public Structure OSDDATA
        ''' <summary>
        ''' Posizione del messaggio OSD
        ''' </summary>
        ''' <remarks>Fare riferimeno a MPC_Enum.OSD_POSITION per l'elenco delle 
        ''' costanti riconosciute</remarks>
        Dim Position As MPC_Enum.OSD_POSITION
        ''' <summary>
        ''' Durata di visualizzazione del messaggio OSD in millisecondi
        ''' </summary>
        ''' <remarks></remarks>
        Dim DurationMS As Integer
        ''' <summary>
        ''' Messaggio (in array di Char) da visualizzare in OSD
        ''' </summary>
        ''' <remarks>Lunghezza massima 128 caratteri</remarks>
        Dim Message() As Char
    End Structure
    ''' <summary>
    ''' Evento generato alla ricezione di un comando non riconosciuto
    ''' </summary>
    ''' <remarks></remarks>
    Public Event MsgReceived()
    ''' <summary>
    ''' Proprietà per ottenere Titolo, Autore, Descrizione, Nome file e Durata 
    ''' dell'ultimo stream aperto
    ''' </summary>
    ''' <value>Proprietà di sola lettura</value>
    ''' <returns>Le informazioni sull'ultimo stream aperto in formato String</returns>
    ''' <remarks>Le informazioni sono raggruppate in MCPToHost.INFOPLAYING: una struttura
    ''' che individua i parametri in formato String.
    ''' La durata è espressa in secondi. Il Nome file include il percorso completo
    ''' </remarks>
    Public ReadOnly Property InfoNowPlaying As MPCToHost.INFOPLAYING
        Get
            Return nowplay
        End Get
    End Property
    ''' <summary>
    ''' Proprietà che restituisce l'ultimo messaggio ricevuto da MPC
    ''' (anche generico/sconosciuto)
    ''' </summary>
    ''' <value>Proprietà di sola lettura</value>
    ''' <returns>La coppia Comando-Messaggio ricevuta strutturati nel tipo MSGIN</returns>
    ''' <remarks>Il messaggio viene compilato durante l'ultimo evento lanciato con 
    ''' la ricezione del messaggio stesso
    ''' </remarks>
    Public ReadOnly Property MessageIn As MSGIN
        Get
            Return msgR
        End Get
    End Property
    ''' <summary>
    ''' Imposta o restituisce il percorso di "mpc-hc.exe"
    ''' </summary>
    ''' <value>Il percorso in formato String del programma MPC</value>
    ''' <returns>Il percorso utilizzato per aprire MPC</returns>
    ''' <remarks>Il percorso è completo e DEVE terminare con "\"</remarks>
    Public Property Path As String
        Get
            Return Percorso
        End Get
        Set(ByVal value As String)
            Percorso = value
        End Set
    End Property
    ''' <summary>
    ''' Proprità che restituisce l'Handle di MPC una volta aperto
    ''' </summary>
    ''' <value>Proprietà di sola lettura</value>
    ''' <returns>L'Handle di MPC dopo l'avvio del programma</returns>
    ''' <remarks>Dopo la chiusura e prima dell'apertura di "mpc-hc.exe"
    ''' il valore ottenuto non è valido
    ''' La riapertura di MPC esternamente non può validare 
    ''' l'Handle qua restituito e per l'utilizzo all'interno della classe MPC
    ''' </remarks>
    Public ReadOnly Property Hndl_MPC As IntPtr
        Get
            Return Hndl
        End Get
    End Property
    ''' <summary>
    ''' Metodo per l'avvio di "mpc-hc.exe" in modalità SLAVE con indicazione dell'Handle
    ''' della classe che comunicherà con MPC
    ''' </summary>
    ''' <remarks>Questo metodo va utilizzato se il percorso del programma è indicato
    ''' globalmente nella variabile di ambiente PATH o successivamente all'impostazione
    ''' della proprietà PATH</remarks>
    Public Sub Run()
        ' Close any other instance.
        Dim ProcessList As Process() = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(AppName))
        For Each item As Process In ProcessList
            Try
                item.CloseMainWindow()
                Thread.Sleep(200)
                If item.HasExited = False Then
                    item.Kill()
                    Thread.Sleep(200)
                End If
            Catch
                Thread.Sleep(200)
            End Try
        Next

        Dim Args As New ProcessStartInfo(System.IO.Path.Combine(Percorso, AppName), "/slave " & Convert.ToString(Comm.HandleFrom))
        Args.WindowStyle = ProcessWindowStyle.Maximized
        MpcProcess = Process.Start(Args)
        MpcProcess = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(AppName)).FirstOrDefault()

        ' Comm.StartPrg(Percorso & "mpc-hc.exe /slave " & Convert.ToString(Comm.HandleFrom))
        RaiseEvent MPC_Running()
    End Sub
    ''' <summary>
    ''' Metodo per l'avvio di "mpc-hc.exe" in modalità SLAVE con indicazione dell'Handle
    ''' della classe che comunicherà con MPC
    ''' </summary>
    ''' <param name="Path">Percorso di "mpc-hc.exe". Il parametro DEVE
    ''' terminare con "\"</param>
    ''' <remarks>Questo metodo va utilizzato per indicare il percorso del programma 
    ''' "mpc-hc.exe". Verrà automaticamente aggiornata la proprietà Path.
    ''' </remarks>
    Public Sub Run(ByVal Path As String)
        Percorso = System.IO.Path.GetDirectoryName(Path)
        Me.AppName = System.IO.Path.GetFileName(Path)
        Run()
    End Sub

    Public Sub SetPath(ByVal Path As String)
        Percorso = System.IO.Path.GetDirectoryName(Path)
        Me.AppName = System.IO.Path.GetFileName(Path)
    End Sub

    ''' <summary>
    ''' Metodo generico per inviare un Comando e il Messaggio relativo in background
    ''' via WM_COPYDATA
    ''' </summary>
    ''' <param name="cmd">Comando da inviare in background</param>
    ''' <param name="param">Parametri legati al comando (Messaggio)</param>
    ''' <remarks>Se non ci sono parametri da inviare, indicare una stringa vuota (param = "")
    ''' Il Comando può essere selezionato tra quelli dichiarati in MPC_Enum.CMD_SEND
    ''' </remarks>
    Public Async Function SendCommandAsync(ByVal cmd As MPC_Enum.CMD_SEND, ByVal param As String) As Task
        RaiseEvent MPC_SendingCommand(cmd, param)
        Await Task.Run(Function() Comm.SendMsg(CommWnd.SYSMSG.WM_COPYDATA, cmd, param))
    End Function

    Private Sub Comm_MsgReceived(ByRef m As System.Windows.Forms.Message) Handles Comm.MsgReceived
        msgR.Message = Comm.lastMsg.TrimEnd(Chr(0))
        msgR.Command = Comm.lastData.dwData
        Select Case msgR.Command
            Case MPC_Enum.CMD_RECEIVED.CMD_CONNECT
                Hndl = Convert.ToString(msgR.Message)
                Comm.HandleTo = Hndl
                RaiseEvent MPC_Connected(msgR)
            Case MPC_Enum.CMD_RECEIVED.CMD_STATE
                Select Case Convert.ToUInt32(msgR.Message)
                    Case MPC_Enum.LOADSTATE.MLS_CLOSED
                        RaiseEvent MPC_Closed(msgR)
                    Case MPC_Enum.LOADSTATE.MLS_CLOSING
                        RaiseEvent MPC_Closing(msgR)
                    Case MPC_Enum.LOADSTATE.MLS_LOADED
                        RaiseEvent MPC_Loaded(msgR)
                    Case MPC_Enum.LOADSTATE.MLS_LOADING
                        RaiseEvent MPC_Loading(msgR)
                    Case Else
                        RaiseEvent MPC_UnknownState(msgR)
                End Select
            Case MPC_Enum.CMD_RECEIVED.CMD_PLAYMODE
                Select Case Convert.ToInt32(msgR.Message)
                    Case MPC_Enum.PLAYSTATE.PS_PLAY
                        RaiseEvent MPC_Play(msgR)
                    Case MPC_Enum.PLAYSTATE.PS_PAUSE
                        RaiseEvent MPC_Pause(msgR)
                    Case MPC_Enum.PLAYSTATE.PS_STOP
                        RaiseEvent MPC_Stop(msgR)
                    Case MPC_Enum.PLAYSTATE.PS_UNUSED
                        RaiseEvent MPC_Unused(msgR)
                    Case Else
                        RaiseEvent MPC_UnknownPlayState(msgR)
                End Select
            Case MPC_Enum.CMD_RECEIVED.CMD_NOWPLAYING
                nowplay = MPCToHost.GetFromNowPlaying(msgR.Message)
                RaiseEvent MPC_NowPlaying(msgR)
            Case MPC_Enum.CMD_RECEIVED.CMD_LISTSUBTITLETRACKS
                RaiseEvent MPC_ListSubtitleTracks(msgR)
            Case MPC_Enum.CMD_RECEIVED.CMD_LISTAUDIOTRACKS
                RaiseEvent MPC_ListAudioTracks(msgR)
            Case MPC_Enum.CMD_RECEIVED.CMD_CURRENTPOSITION
                RaiseEvent MPC_CurrentPosition(msgR)
            Case MPC_Enum.CMD_RECEIVED.CMD_NOTIFYSEEK
                RaiseEvent MPC_NotifySeek(msgR)
            Case MPC_Enum.CMD_RECEIVED.CMD_NOTIFYENDOFSTREAM
                RaiseEvent MPC_NotifyEndOfStream(msgR)
            Case MPC_Enum.CMD_RECEIVED.CMD_PLAYLIST
                RaiseEvent MPC_Playlist(msgR)
            Case Else
                RaiseEvent MsgReceived()
        End Select
        RaiseEvent MPC_GENERIC(msgR)
    End Sub
    ''' <summary>
    ''' Metodo per l'apertura di un file su MPC
    ''' </summary>
    ''' <param name="PathName">Nome e percorso del file</param>
    ''' <remarks>Questo comando determina in ordine gli eventi
    ''' Loading, Loaded, NowPlaying, ListSubtitleTracks, ListAudioTracks, Pause, Play</remarks>
    Public Async Function OpenFileAsync(ByVal PathName As String) As Task
        Await SendCommandAsync(CMD_SEND.CMD_OPENFILE, PathName)
    End Function

    ''' <summary>
    ''' Metodo per inviare il comando STOP e MANTIENI ad MPC
    ''' </summary>
    ''' <remarks>Lo stream in uso verrà fermato e portato al secondo 0 senza 
    ''' la chiusura
    ''' Determina, in ordine, gli eventi Stop</remarks>
    Public Async Function StopAndKeepAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_STOP, "")
    End Function
    ''' <summary>
    ''' Metodo per inviare il comando STOP e CHIUDI ad MPC
    ''' </summary>
    ''' <remarks>Lo stream in uso verrà fermato e chiuso
    ''' Determina, in ordine, gli eventi Closing, Closing, Stop e Closed</remarks>
    Public Async Function StopAndCloseAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_CLOSEFILE, "")
    End Function
    ''' <summary>
    ''' Metodo per inviare il comando Play/Pause ad MPC
    ''' </summary>
    ''' <remarks>Se in stato di Play: manda in stato Pause e determina, in ordine,
    ''' gli eventi Pause
    ''' Se in stato di Pause: manda in stato Play e determina, in ordine, gli eventi 
    ''' Play</remarks>
    Public Async Function PlayPauseAsync() As task
        Await SendCommandAsync(CMD_SEND.CMD_PLAYPAUSE, "")
    End Function
    ''' <summary>
    ''' Metodo per aggiungere un file in Playlist su MPC
    ''' </summary>
    ''' <param name="PathName">Nome e percorso file da aggiungere</param>
    ''' <remarks>Non determina eventi</remarks>
    Public Async Function AddToPlaylist(ByVal PathName As String) As Task
        Await SendCommandAsync(CMD_SEND.CMD_ADDTOPLAYLIST, PathName)
    End Function
    ''' <summary>
    ''' Metodo per inviare il comando di svuotamento della Playlist su MPC
    ''' </summary>
    ''' <remarks>Non determina eventi</remarks>
    Public Async Function ClearPlaylistAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_CLEARPLAYLIST, "")
    End Function
    ''' <summary>
    ''' Metodo per inviare il comando di Inizio riproduzione degli elementi in 
    ''' Playlist (dal primo) su MPC
    ''' </summary>
    ''' <remarks>Determina, in ordine, gli eventi Closing, Closing, Stop, Closed,
    ''' Loading, Loaded, NowPlaying, ListSubtitleTracks, ListAudioTracks, Pause e Play
    ''' </remarks>
    Public Async Function StartPlaylist() As Task
        Await SendCommandAsync(CMD_SEND.CMD_STARTPLAYLIST, "")
    End Function
    ''' <summary>
    ''' Metodo per la rimozione di un elemento in Playlist su MPC
    ''' NON ANCORA IMPLEMENTATO NELLE API MPC
    ''' </summary>
    ''' <remarks>Non funzionante; dichiarato non implementato</remarks>
    Public Sub RemoveFromPlaylist()
        'TODO
    End Sub
    ''' <summary>
    ''' Metodo per inviare il comando per impostare la posizione in secondi sullo 
    ''' stream corrente in MPC
    ''' </summary>
    ''' <param name="PositionSec">Nuova posizione in secondi</param>
    ''' <remarks>Determina, in ordine, gli eventi Pausa, NotifySeek e Play</remarks>
    Public Async Function SetPositionAsync(ByVal PositionSec As Integer) As Task
        Await SendCommandAsync(CMD_SEND.CMD_SETPOSITION, Convert.ToString(PositionSec))
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando per ritardare il flusso audio (rispetto al video)
    ''' su MPC
    ''' </summary>
    ''' <param name="Delay">Ritardo specificato in millisecondi</param>
    ''' <remarks>Non determina eventi</remarks>
    Public Async Function SetAudioDelayAsync(ByVal Delay As Integer) As Task
        Await SendCommandAsync(CMD_SEND.CMD_SETAUDIODELAY, Convert.ToString(Delay))
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando per ritardare la visualizzazione dei sottotitoli
    ''' su MPC
    ''' </summary>
    ''' <param name="Delay">Ritardo espresso in millisecondi</param>
    ''' <remarks>Non determina eventi</remarks>
    Public Async Function SetSubtitleDelayAsync(ByVal Delay As Integer) As Task
        Await SendCommandAsync(CMD_SEND.CMD_SETSUBTITLEDELAY, Convert.ToString(Delay))
    End Function
    ''' <summary>
    ''' Metodo per inviare il comando di settaggio del file attivo in Playlist su MPC
    ''' COMANDO NON FUNZIONANTE
    ''' </summary>
    ''' <param name="index">Indice del file da settare come attivo</param>
    ''' <remarks>COMANDO NON FUNZIONANTE</remarks>
    Public Async Function SetActiveFileInPlaylistAsync(ByVal index As Integer) As Task
        'Doesn't work
        Await SendCommandAsync(CMD_SEND.CMD_SETINDEXPLAYLIST, Convert.ToString(index))
    End Function
    ''' <summary>
    ''' Metodo per inviare il comando per togliere lo stato attivo all'elemento attivo
    ''' in Playlist su MPC
    ''' </summary>
    ''' <remarks>Non determina eventi. Non testato.</remarks>
    Public Async Function SetNoActiveFileInPlaylist() As Task
        Await SetActiveFileInPlaylistAsync(-1)
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando per settare lo stato attivo su una traccia
    ''' audio su MPC
    ''' </summary>
    ''' <param name="index">Indice della traccia audio da attivare</param>
    ''' <remarks>Non determina eventi. Non testato.</remarks>
    Public Async Function SetActiveAudioTrackAsync(ByVal index As Integer) As Task
        Await SendCommandAsync(CMD_SEND.CMD_SETAUDIOTRACK, Convert.ToString(index))
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando per settare lo stato attivo su una traccia
    ''' sottotitolo su MPC
    ''' </summary>
    ''' <param name="index">Indice della traccia sottotitolo da attivare</param>
    ''' <remarks>Non determina eventi. Non testato.</remarks>
    Public Async Function SetActiveSubtitleTrackAsync(ByVal index As Integer) As Task
        Await SendCommandAsync(CMD_SEND.CMD_SETSUBTITLETRACK, Convert.ToString(index))
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di disattivazione dei sottotitoli su MPC
    ''' </summary>
    ''' <remarks>Non determina eventi. Non testato.</remarks>
    Public Async Function SetSubtitlesDisabledAsync() As Task
        Await SetActiveSubtitleTrackAsync(-1)
    End Function
    ''' <summary>
    ''' Metodo per l'invio della richiesta della lista delle tracce sottotitolo
    ''' </summary>
    ''' <remarks>Determina l'evento ListSubtitleTracks</remarks>
    Public Async Function GetSubtitleTracksAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_GETSUBTITLETRACKS, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio della richiesta della posizione corrente sullo stream attivo
    ''' </summary>
    ''' <remarks>Determina l'evento CurrentPosition</remarks>
    Public Async Function GetCurrentPositionAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_GETCURRENTPOSITION, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio della richiesta della lista delle tracce audio
    ''' </summary>
    ''' <remarks>Determina l'evento ListAudioTracks</remarks>
    Public Async Function GetAudioTracksAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_GETAUDIOTRACKS, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio della richiesta delle informazioni (Titolo, Autore, 
    ''' Descrizione, Nomefile, Durata) sullo stream corrente
    ''' </summary>
    ''' <remarks>Dovrebbe determinare l'evento NowPlaying. NON FUNZIONANTE</remarks>
    Public Async Function GetNowPlayingAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_GETNOWPLAYING, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio della richiesta della lista dei file in Playlist su MPC
    ''' </summary>
    ''' <remarks>Determina l'evento PlayList</remarks>
    Public Async Function GetPlaylistAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_GETPLAYLIST, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di salto avanti di un intervallo di tempo 
    ''' specificato per lo stream corrente
    ''' </summary>
    ''' <param name="Seconds">Intervallo di tempo espresso in secondi</param>
    ''' <remarks>Determina, in ordine, gli eventi Pause, NotifySeek e Play</remarks>
    Public Async Function JumpForwardAsync(ByVal Seconds As Integer) As Task
        Await SendCommandAsync(CMD_SEND.CMD_JUMPOFNSECONDS, Convert.ToString(Seconds))
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di salto indietro di un intervallo di tempo 
    ''' specificato per lo stream corrente
    ''' </summary>
    ''' <param name="Seconds">Intervallo di tempo espresso in secondi</param>
    ''' <remarks>Determina, in ordine, gli eventi Pause, NotifySeek e Play</remarks>
    Public Async Function JumpBackwardAsync(ByVal Seconds As Integer) As Task
        Await SendCommandAsync(CMD_SEND.CMD_JUMPOFNSECONDS, Convert.ToString(0 - Seconds))
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di salto avanti breve per lo stream corrente
    ''' </summary>
    ''' <remarks>Determina l'evento NotifySeek</remarks>
    Public Async Function JumpForwardAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_JUMPFORWARDMED, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di salto indietro breve per lo stream corrente
    ''' </summary>
    ''' <remarks>Determina l'evento NotifySeek</remarks>
    Public Async Function JumpBackwardAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_JUMPBACKWARDMED, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di switch della modalità Schermo intero su MPC
    ''' </summary>
    ''' <remarks>Non determina eventi.
    ''' Lo schermo intero viene aperto sullo schermo in cui è attivo MPC.
    ''' </remarks>
    Public Async Function ToggleFullScreenAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_TOGGLEFULLSCREEN, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di aumento del volume audio su MPC
    ''' </summary>
    ''' <remarks>Non determina eventi</remarks>
    Public Async Function IncreaseVolumeAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_INCREASEVOLUME, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di diminuzione del volume audio su MPC
    ''' </summary>
    ''' <remarks>Non determina eventi</remarks>
    Public Async Function DecreaseVolumeAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_DECREASEVOLUME, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando di switch della modalità Shader su MPC
    ''' </summary>
    ''' <remarks>Non determina eventi</remarks>
    Public Async Function ToggleShaderAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_SHADER_TOGGLE, "")
    End Function
    ''' <summary>
    ''' Metodo per la chiusura di MPC
    ''' </summary>
    ''' <remarks>Dopo la chiusura di MPC la proprietà Hndl_MPC viene invalidata.
    ''' Inoltre, non è possibile ricevere comandi e l'invio non andrà a buon fine.
    ''' Determina, in ordine, gli eventi Closing, Closing, Stop e Closed</remarks>
    Public Async Function CloseMpcAsync() As Task
        Await SendCommandAsync(CMD_SEND.CMD_CLOSEAPP, "")
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando che permette di visualizzare un messaggio OSD
    ''' su MPC
    ''' </summary>
    ''' <param name="msg">Messaggio in formato OSDDATA contenente Posizione, Durata e
    ''' Messaggio da visualizzare</param>
    ''' <remarks>Non determina eventi. Fare riferimento alla struttura dichiarata
    ''' OSDDATA.
    ''' Non completamente testata.
    ''' Il messaggio viene visualizzato esclusivamente se l'OSD è attivato nelle opzioni
    ''' MPC. In caso contrario non ci saranno effetti.</remarks>
    Public Async Function ShowOSDMessageAsync(ByVal msg As OSDDATA) As Task
        Dim str As String = Convert.ToString(msg.Position) & "|" & _
            Convert.ToString(msg.DurationMS) & _
            Convert.ToString(msg.Message)
        Await SendCommandAsync(CMD_SEND.CMD_OSDSHOWMESSAGE, str)
    End Function
    ''' <summary>
    ''' Metodo per l'invio del comando che permette di visualizzare un messaggio OSD
    ''' su MPC
    ''' </summary>
    ''' <param name="pos">Posizione sullo schermo del messaggio (fare riferimento alle
    ''' costanti MPC_Enum.OSD_POSITION</param>
    ''' <param name="dur">Durata di visualizzazione espressa in millisecondi</param>
    ''' <param name="msg">Messaggio da visualizzare</param>
    ''' <remarks>Non determina eventi.
    ''' Non completamente testata.
    ''' Il messaggio viene visualizzato esclusivamente se l'OSD è attivato nelle opzioni
    ''' MPC. In caso contrario non ci saranno effetti.</remarks>
    Public Async Function ShowOSDMessageAsync(ByVal pos As Integer, ByVal dur As Integer, ByVal msg As String) As Task
        Dim str As String = Convert.ToString(pos) & "|" & _
            Convert.ToString(dur) & _
            Convert.ToString(msg)
        Await SendCommandAsync(CMD_SEND.CMD_OSDSHOWMESSAGE, str)
    End Function
End Class
