using System;            
using System.Net;           
using System.Net.Sockets;
using System.Security;
using System.Text;                   
using System.Threading;      
using System.Data;
using System.Diagnostics;

class SnakeServer {
    //public static List<Lobby> players = new List<Lobby>();
    static TcpClient[] playersBuffer = new TcpClient[2];
    

    static void Main() {
        
       
        TcpListener listener = new TcpListener(IPAddress.Any, 48080);
        listener.Start(); 
        Console.WriteLine("Сервер запущен...");

        while (true) {
            Lobby lobby = PlayersConnection(listener);

            Thread t = new Thread(() => Game(lobby));
            
            t.Start(); 
        }
    }

    static Lobby PlayersConnection(TcpListener listener){
        for (int i = 0; i < 2; i++)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("player " + i + " connected");
                
                playersBuffer[i] = client;
                
            }
            //players.Add(new Lobby(playersBuffer[0], playersBuffer[1]));
            Console.WriteLine("lobby created");



        return new Lobby(playersBuffer[0], playersBuffer[1]);
    }

    static async Task SendStartParam(NetworkStream player1, NetworkStream player2){
        string msg = "play,1";

        byte[] responseBytes = Encoding.UTF8.GetBytes(msg);
        player1.Write(responseBytes, 0, responseBytes.Length);
        msg = "play,2";
        responseBytes = Encoding.UTF8.GetBytes(msg);
        player2.Write(responseBytes, 0, responseBytes.Length);
    }


    static async Task Game(Lobby lobby) {
        try {
            NetworkStream stream1 = lobby.player1.GetStream();
            NetworkStream stream2 = lobby.player2.GetStream();

            SendStartParam(stream1, stream2);
            

            
            Task task1 = Reading(stream1, stream2);
            Task task2 = Reading(stream2, stream1);

            await Task.WhenAll(task1, task2);
        }
        catch (Exception ex) {
            Console.WriteLine($"Game ERROR: {ex.Message}");
        }
        finally {
            lobby.player1.Close();
            lobby.player2.Close();
            Console.WriteLine("players disconnected");
        }
    }

static async Task Reading(NetworkStream from, NetworkStream to) {
    try {
        byte[] buffer = new byte[1024];
        int byteCount;

        while ((byteCount = await from.ReadAsync(buffer, 0, buffer.Length)) != 0) {

            string msg = Encoding.UTF8.GetString(buffer, 0, byteCount);
            Console.WriteLine($": {msg}");

            byte[] responseBytes = Encoding.UTF8.GetBytes(msg);
            await to.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        
    }
    catch (Exception ex) {
        Console.WriteLine($"Reading ERROR: {ex.Message}");
    }
}


 }
class Lobby{
        public TcpClient player1;
        public TcpClient player2;

        public Lobby(TcpClient player1, TcpClient player2){
            this.player1 = player1;
            this.player2 = player2;
        }

    }


