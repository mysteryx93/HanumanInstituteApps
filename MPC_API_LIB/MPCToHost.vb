Public Class MPCToHost
    ''' <summary>
    ''' Struttura che identifica il gruppo di Informazioni restituite relative
    ''' allo stream corrente: Titolo, Autore, Descrizione, Nome file, Durata
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure INFOPLAYING
        ''' <summary>
        ''' Info Titolo stream
        ''' </summary>
        ''' <remarks></remarks>
        Dim Title As String
        ''' <summary>
        ''' Info Autore stream
        ''' </summary>
        ''' <remarks></remarks>
        Dim Author As String
        ''' <summary>
        ''' Info Descrizione stream
        ''' </summary>
        ''' <remarks></remarks>
        Dim Description As String
        ''' <summary>
        ''' Info Nome e percorso file stream
        ''' </summary>
        ''' <remarks></remarks>
        Dim PathName As String
        ''' <summary>
        ''' Info Durata stream
        ''' </summary>
        ''' <remarks></remarks>
        Dim Duration As String
    End Structure

    ''' <summary>
    ''' Funzione di conversione dell'output del messaggio Connect
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Handle di MPC in formato Int32</returns>
    ''' <remarks></remarks>
    Public Shared Function GetFromConnect(ByVal msg As String) As Integer
        Return Convert.ToInt32(msg)
    End Function
    ''' <summary>
    ''' Funzione di conversione dell'output del messaggio NowPlaying
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Oggetto INFOPLAYING contentente Titolo, Autore, Descrizione,
    ''' Nome file e Durata</returns>
    ''' <remarks>Tutti gli output sono di tipo String (anche la durata in secondi)</remarks>
    Public Shared Function GetFromNowPlaying(ByVal msg As String) As INFOPLAYING
        Dim x As Integer = 0
        Dim precindex As Integer = 0
        Dim str(4) As String
        For i As Integer = 0 To msg.Length - 1
            If msg(i) = "|" Then
                If i = 0 Then
                    str(x) = ""
                    x += 1
                    precindex = 1
                Else
                    If msg(i - 1) <> "\" Then
                        str(x) = msg.Substring(precindex, i - precindex)
                        x += 1
                        precindex = i + 1
                    End If
                End If
            ElseIf i = msg.Length - 1 Then
                str(x) = msg.Substring(precindex, i - precindex)
            End If
            If x > 4 Then x = 4
        Next
        GetFromNowPlaying.Title = str(0)
        GetFromNowPlaying.Author = str(1)
        GetFromNowPlaying.Description = str(2)
        GetFromNowPlaying.PathName = str(3)
        GetFromNowPlaying.Duration = str(4)
    End Function
    ''' <summary>
    ''' Funzione di conversione dell'output del messaggio ListSubtitleTracks
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Array di stringhe contenenti ognuna il titolo di una traccia 
    ''' sottotitolo. Se non ci sono sottotitoli viene restituito NOTHING.
    ''' L'ultimo elemento dell'array è quello attivo in MPC.
    ''' </returns>
    ''' <remarks></remarks>
    Public Shared Function GetFromListSubtitleTracks(ByVal msg As String) As String()
        If msg.StartsWith("-1") Then Return Nothing
        If msg.StartsWith("-2") Then Return Nothing
        Dim x As Integer = 0
        Dim precindex As Integer = 0
        Dim out() As String = Nothing
        For i As Integer = 0 To msg.Length - 1
            If msg(i) = "|" Then
                If i = 0 Then
                    ReDim Preserve out(0)
                    out(x) = ""
                    x += 1
                    precindex = 1
                Else
                    If msg(i - 1) <> "\" Then
                        If out Is Nothing Then ReDim Preserve out(0) Else ReDim Preserve out(out.Length)
                        out(x) = msg.Substring(precindex, i - precindex)
                        ReDim Preserve out(out.Length)
                        x += 1
                        precindex = i + 1
                    End If
                End If
            ElseIf i = msg.Length - 1 Then
                out(x) = msg.Substring(precindex, i - precindex)
            End If
        Next
        Return out
    End Function
    ''' <summary>
    ''' Funzione che restituisce l'elemento attivo sulla lista delle tracce sottotitolo
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Stringa contenente il nome della traccia sottotitolo attiva.
    ''' Se non ci sono tracce sottotitolo o nessuna è caricata viene restituito NOTHING</returns>
    ''' <remarks></remarks>
    Public Shared Function GetActiveFromListSubtitleTracks(ByVal msg As String) As String
        Dim out() As String = GetFromListSubtitleTracks(msg)
        If out Is Nothing Then Return Nothing
        Return out(0)
    End Function
    ''' <summary>
    ''' Funzione di conversione dell'output del messaggio ListAudioTracks
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Array di stringhe contenenti ognuna il titolo di una traccia 
    ''' audio. Se non ci sono tracce audio viene restituito NOTHING.
    ''' L'ultimo elemento dell'array è quello attivo in MPC.
    ''' </returns>
    ''' <remarks></remarks>
    Public Shared Function GetFromListAudioTracks(ByVal msg As String) As String()
        Return GetFromListSubtitleTracks(msg)
    End Function
    ''' <summary>
    ''' Funzione che restituisce l'elemento attivo sulla lista delle tracce audio
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Stringa contenente il nome della traccia audio attiva.
    ''' Se non ci sono tracce audio o nessuna è caricata viene restituito NOTHING</returns>
    ''' <remarks></remarks>
    Public Shared Function GetActiveFromListAudioTracks(ByVal msg As String) As String
        Return GetActiveFromListSubtitleTracks(msg)
    End Function
    ''' <summary>
    ''' Funzione di conversione dell'output del messaggio CurrentPosition
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Valore Int32 contenente la posizione corrente in secondi sullo stream</returns>
    ''' <remarks></remarks>
    Public Shared Function GetFromCurrentPosition(ByVal msg As String) As Integer
        Return Convert.ToInt32(msg)
    End Function
    ''' <summary>
    ''' Funzione di conversione dell'output del messaggio NotifySeek
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Valore Int32 contenente la posizione corrente in secondi sullo stream</returns>
    ''' <remarks></remarks>
    Public Shared Function GetFromNotifySeek(ByVal msg As String) As Integer
        Return GetFromCurrentPosition(msg)
    End Function
    ''' <summary>
    ''' Funzione di conversione dell'output del messaggio Playlist
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Array di stringhe contenenti ognuna il titolo di una traccia in 
    ''' playlist. Se non ci sono tracce viene restituito NOTHING.
    ''' L'ultimo elemento dell'array è quello attivo in MPC.
    ''' </returns>
    ''' <remarks></remarks>
    Public Shared Function GetFromPlaylist(ByVal msg As String) As String()
        Return GetFromListSubtitleTracks(msg)
    End Function
    ''' <summary>
    ''' Funzione che restituisce l'elemento attivo sulla lista delle tracce playlist
    ''' </summary>
    ''' <param name="msg">Stringa del messaggio inviato da MPC</param>
    ''' <returns>Stringa contenente il nome della traccia attiva.
    ''' Se non ci sono tracce o nessuna è caricata viene restituito NOTHING</returns>
    ''' <remarks></remarks>
    Public Shared Function GetActiveFromPlaylist(ByVal msg As String) As String
        Return GetActiveFromListSubtitleTracks(msg)
    End Function

End Class
