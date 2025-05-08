using System; 
using System.Net.Sockets;
using System.Text;


namespace MyProject.Client{

    public class SnakeClient {
        TcpClient client;
        NetworkStream stream;
        byte[] buffer;
        int byteCount;
        byte[] data;

        public SnakeClient(){
            client = new TcpClient("127.0.0.1", 48080); 
            stream = client.GetStream(); 
        }

        public bool SendRequest(string request){
            data = Encoding.UTF8.GetBytes(request);
            stream.Write(data, 0, data.Length);

            return true;
        }

        public string GetRequest(){
            buffer = new byte[1024];
            byteCount = stream.Read(buffer, 0, buffer.Length);
            if(byteCount > 0){
                return Encoding.UTF8.GetString(buffer, 0, byteCount);
            }else{
                return "null";
            }
            
        }

        
    }
}
