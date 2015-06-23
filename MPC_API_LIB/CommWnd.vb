Imports System.Runtime.InteropServices

Public Class CommWnd
    ''' <summary>
    ''' Elenco delle costanti dei canali di comunicazione tra processi 
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum SYSMSG As Integer
        ''' <summary>
        ''' Canale WM_COPYDATA
        ''' </summary>
        ''' <remarks>Prevede un parametro strutturato in 
        ''' COMANDO-MESSAGGIO-DIMENSIONE_MESSAGGIO
        ''' Si veda la struttura CopyData</remarks>
        WM_COPYDATA = &H4A
    End Enum
    ''' <summary>
    ''' Struttura CopyData per il passaggio dei parametri del canale WM_COPYDATA
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure CopyData
        ''' <summary>
        ''' Comando da inviare
        ''' </summary>
        ''' <remarks></remarks>
        Public dwData As IntPtr
        ''' <summary>
        ''' Dimensione del messaggio contenente i parametri del Comando
        ''' </summary>
        ''' <remarks></remarks>
        Public cbData As Integer
        ''' <summary>
        ''' Messaggio contenente i parametri per la fuzione Comando
        ''' </summary>
        ''' <remarks></remarks>
        Public lpData As IntPtr
    End Structure

    ''' <summary>
    ''' Evento generato alla ricezione di un messaggio su canale WM_COPYDATA
    ''' </summary>
    ''' <param name="m">Messaggio ricevuto di tipo CopyData</param>
    ''' <remarks></remarks>
    Public Event MsgReceived(ByRef m As Windows.Forms.Message)

    ''' <summary>
    ''' Funzione API di sistema per l'invio di messaggi ai processi in background
    ''' </summary>
    ''' <param name="hWnd">Handle del processo di destinazione</param>
    ''' <param name="Msg">Canale di comunicazione</param>
    ''' <param name="wParam">Handle del processo mittente</param>
    ''' <param name="lParam">Comando e Messaggio passato al processo</param>
    ''' <returns>True se l'operazione va a buon fine, False altrimenti</returns>
    ''' <remarks></remarks>
    Public Declare Auto Function SendMessage Lib "user32" (ByVal hWnd As IntPtr, _
                                                            ByVal Msg As Integer, _
                                                            ByVal wParam As IntPtr, _
                                                            ByRef lParam As CopyData) As Boolean

    Private lastMsgReceived As String = ""
    Private lastDataReceived As CopyData = Nothing
    Private hwnd_to As IntPtr = Nothing
    Private hwnd_from As IntPtr = Me.Handle
    Private dataSend As CopyData = Nothing

    ''' <summary>
    ''' Proprietà che restituisce il messaggio passato con la funzione lastData 
    ''' attraverso il canale WM_COPYDATA
    ''' </summary>
    ''' <value>Proprietà di sola lettura</value>
    ''' <returns>Stringa contenente il messaggio passato dal processo remoto</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property lastMsg As String
        Get
            Return lastMsgReceived
        End Get
    End Property
    ''' <summary>
    ''' Proprietà che restituisce l'ultima l'intera funzione e parametri ricevuti
    ''' attraverso il canale WM_COPYDATA
    ''' </summary>
    ''' <value>Proprietà di sola lettura</value>
    ''' <returns>Oggetto CopyData contenente l'ultimo messaggio ricevuto</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property lastData As CopyData
        Get
            Return lastDataReceived
        End Get
    End Property
    ''' <summary>
    ''' Proprietà che imposta/restituisce l'Handle del processo Host (che controlla)
    ''' </summary>
    ''' <value>Imposta l'Handle del processo Host</value>
    ''' <returns>L'Handle utilizzato per il processo Host</returns>
    ''' <remarks></remarks>
    Public Property HandleFrom As IntPtr
        Get
            Return hwnd_from
        End Get
        Set(ByVal value As IntPtr)
            hwnd_from = value
        End Set
    End Property
    ''' <summary>
    ''' Proprietà che imposta/restituisce l'Handle del processo Remoto (controllato)
    ''' </summary>
    ''' <value>Imposta l'Handle del processo Remoto</value>
    ''' <returns>L'Handle utilizzato per il processo Remoto</returns>
    ''' <remarks></remarks>
    Public Property HandleTo As IntPtr
        Get
            Return hwnd_to
        End Get
        Set(ByVal value As IntPtr)
            hwnd_to = value
        End Set
    End Property
    ''' <summary>
    ''' Proprietà che imposta/restituisce l'oggetto CopyData da passare per il canale
    ''' WM_COPYDATA
    ''' </summary>
    ''' <value>Specifica l'oggetto CopyData contenente comando-parametri da inviare</value>
    ''' <returns>Restituisce l'oggetto CopyData impostato, contenente 
    ''' comando-parametri da inviare</returns>
    ''' <remarks></remarks>
    Public Property DataToSend As CopyData
        Get
            Return dataSend
        End Get
        Set(ByVal value As CopyData)
            dataSend = value
        End Set
    End Property

    ''' <summary>
    ''' Metodo utilizzato per avviare un processo da utilizzare come processo Remoto
    ''' </summary>
    ''' <param name="PathName">Nome e percorso file eseguibile (ed eventuali parametri
    ''' su riga di comando)</param>
    ''' <param name="Style">Specifica lo stile della finestra e il focus</param>
    ''' <param name="Wait">True per attendere la chiusura del processo da avviare; 
    ''' False per rilasciare immediatamente il thread d'esecuzione del processo da avviare
    ''' e continuare l'esecuzione del programma</param>
    ''' <param name="Timeout">Tempo in millisecondi d'attesa dopo la chiusura del processo
    ''' d'avviare prima di restituire il controllo al programma (se Wait è True).
    ''' -1 indica che non vi è alcun timeout</param>
    ''' <remarks>Per i parametri vedere anche la funzione Shell</remarks>
    Public Sub StartPrg(ByVal PathName As String, Optional ByVal Style As AppWinStyle = AppWinStyle.NormalNoFocus, Optional ByVal Wait As Boolean = False, Optional ByVal Timeout As Integer = -1)
        Shell(PathName, Style, Wait, Timeout)
    End Sub

    ''' <summary>
    ''' Funzione che compatta Comando-Parametri nell'oggetto CopyData
    ''' </summary>
    ''' <param name="cmd">Comando per il canale WM_COPYDATA</param>
    ''' <param name="msg">Parametri per il comando</param>
    ''' <returns>Oggetto CopyData per il canale WM_COPYDATA</returns>
    ''' <remarks></remarks>
    Public Function packMsg(ByVal cmd As Integer, ByVal msg As String) As CopyData
        packMsg.dwData = cmd
        packMsg.cbData = (msg.Length + 1) * Marshal.SystemDefaultCharSize
        packMsg.lpData = Marshal.StringToHGlobalUni(msg)
    End Function

    ''' <summary>
    ''' Funzione per l'invio di messaggi ad un processo Remoto
    ''' </summary>
    ''' <param name="SysMsg">Canale di invio</param>
    ''' <param name="cmd">Comando</param>
    ''' <param name="msg">Parametri del comando</param>
    ''' <returns>True se è andata a buon fine; False altrimenti</returns>
    ''' <remarks></remarks>
    Public Function SendMsg(ByVal SysMsg As SYSMSG, ByVal cmd As Integer, ByVal msg As String) As Boolean
        Return SendMessage(hwnd_to, SysMsg, hwnd_from, packMsg(cmd, msg))
    End Function
    ''' <summary>
    ''' Funzione per l'invio di messaggi ad un processo Remoto
    ''' </summary>
    ''' <param name="handleTo">Handle del processo Remoto</param>
    ''' <param name="SysMsg">Canale di invio</param>
    ''' <param name="cmd">Comando</param>
    ''' <param name="msg">Parametri del comando</param>
    ''' <returns>True se è andata a buon fine; False altrimenti</returns>
    ''' <remarks></remarks>
    Public Function SendMsg(ByVal handleTo As Integer, ByVal SysMsg As SYSMSG, ByVal cmd As Integer, ByVal msg As String) As Boolean
        Return SendMessage(handleTo, SysMsg, hwnd_from, packMsg(cmd, msg))
    End Function
    ''' <summary>
    ''' Funzione per l'invio di messaggi ad un processo Remoto
    ''' </summary>
    ''' <param name="handleTo">Handle del processo Remoto</param>
    ''' <param name="handleFrom">Handle del processo Host</param>
    ''' <param name="SysMsg">Canale di invio</param>
    ''' <param name="cmd">Comando</param>
    ''' <param name="msg">Parametri del comando</param>
    ''' <returns>True se è andata a buon fine; False altrimenti</returns>
    ''' <remarks></remarks>
    Public Function SendMsg(ByVal handleTo As Integer, ByVal handleFrom As Integer, ByVal SysMsg As SYSMSG, ByVal cmd As Integer, ByVal msg As String) As Boolean
        Return SendMessage(handleTo, SysMsg, handleFrom, packMsg(cmd, msg))
    End Function

End Class
