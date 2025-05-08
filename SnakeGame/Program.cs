namespace MyProject;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.VisualBasic;
using MyProject.DataBase;
using MyProject.Client;
using MyProject.SnakeOnline;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;

class Point{
    public int x;
    public int y;

    public Point(int xx, int yy){
        x = xx;
        y = yy;
    }
}

class Snake{

    public List<Point> snake;
    public Direction direction;
    public Point nextPoint = new Point(0,0);
    public Point temp = new Point(0,0);
    public Point temp2 = new Point(0,0);
    //public char direction; 
    public enum Direction{
            Up,
            Down,
            Right,
            Left
        }
    
}

class Food{
    public Random random = new Random();
    public Point food = new Point(5,5);
}

class Player{
    public string nickname = "guest";
    public int score = 0;
    public int maxScore = 0;
    public bool isHost = true;

    public bool Login(string login){
        if(login.Length < 2 || login.Length > 10){
            return false;
        }
        return true;
    }

    public bool DBScoreRead(){
        DBase db = new DBase();
        db.OpenConnection();
        DataTable table = new DataTable();
        MySqlDataAdapter adapter = new MySqlDataAdapter();

        MySqlCommand command = new MySqlCommand("SELECT * FROM Scores WHERE nickName = @nickname", db.GetConnection()); 

        command.Parameters.Add("@nickname", MySqlDbType.VarChar).Value = nickname;
        adapter.SelectCommand = command;
        adapter.Fill(table);

        if(table.Rows.Count > 0){
            maxScore = table.Rows[0]["score"] is DBNull
            ? 0
            : Convert.ToInt32(table.Rows[0]["score"].ToString());

            db.CloseConnection();
            return true;
            
        }

        db.CloseConnection();

        return false;
    }

    public string DBTopScoresRead(){
        StringBuilder sb = new StringBuilder();
        DBase db = new DBase();
        db.OpenConnection();
        DataTable table = new DataTable();
        MySqlDataAdapter adapter = new MySqlDataAdapter();

        MySqlCommand command = new MySqlCommand("SELECT * FROM Scores ORDER BY score DESC LIMIT 5", db.GetConnection()); 

        adapter.SelectCommand = command;
        adapter.Fill(table);

        if(table.Rows.Count > 0){
            for (int i = 0; i < table.Rows.Count; i++)
            {
                sb.AppendLine(i+1+": " + table.Rows[i]["nickName"].ToString()+ " - "+ table.Rows[i]["score"].ToString());
            }

            
            db.CloseConnection();
            return sb.ToString();
            
        }

        db.CloseConnection();
        return "error";
    }
    public bool isPlayerExist(){
        DBase db = new DBase();
        db.OpenConnection();
        DataTable table = new DataTable();
        MySqlDataAdapter adapter = new MySqlDataAdapter();

        MySqlCommand command = new MySqlCommand("SELECT * FROM Scores WHERE nickName = @nickname", db.GetConnection()); 

        command.Parameters.Add("@nickname", MySqlDbType.VarChar).Value = nickname;
        adapter.SelectCommand = command;
        adapter.Fill(table);

        if(table.Rows.Count > 0){

            db.CloseConnection();
            return true;            
        }

        db.CloseConnection();
        return false;
    }

    public bool DBScoreSave(){
        DBase db = new DBase();
        db.OpenConnection();
        MySqlCommand command = command = new MySqlCommand(@"
            INSERT INTO Scores (nickName, score)
            VALUES (@nickname, @score)
            ON DUPLICATE KEY UPDATE score = @score;
        ", db.GetConnection());

        
        command.Parameters.Add("@nickname", MySqlDbType.VarChar).Value = nickname;
        command.Parameters.Add("@score", MySqlDbType.Int32).Value = score;

        command.ExecuteNonQuery();

        db.CloseConnection();

        return true;
    }    
}

class GameOnline : IOnlineGame{
    StringBuilder sb = new StringBuilder();

    public bool OnlineGetStartParam(string startsParam, Player player){
        if(startsParam == "isHost"){
            player.isHost = true;
        }
        return true;
    }
    public string Serealization(Snake snake){
        string gg = snake.direction.ToString();

        return $"d,{gg}.";
    }

    public string SnakeSerealization(Snake snake){
        sb.Clear();
        sb.Append($"s,{snake.direction},{snake.temp2.x},{snake.temp2.y}");
        foreach(Point point in snake.snake){
            sb.Append($",{point.x},{point.y}");
        }
        sb.Append(".");
        return sb.ToString();
    }

    public string FoodSerealization(Food food){
        return $"f,{food.food.x},{food.food.y}.";
    }


    
}
class DataLayer{
    
}
class Game {
    string deserealizationString = " ";
    bool isOnline = false;
    bool play = true;
    GameOnline gameOnline = new GameOnline();
    SnakeClient snakeClient;
    DataLayer dataLayer = new DataLayer();
        Player player = new Player();
        GameField gameField = new GameField();
        Snake snake = new Snake();  
        Snake snakeOpponent = new Snake(); // 2 player

        Food food = new Food();
        ConsoleDrawing consoleDrawing = new ConsoleDrawing();

        int delay = 500;
        
        public Game(){
            
        }
    

    bool StartGame(){
        consoleDrawing.StartScreen(player, dataLayer, ref isOnline);
        switch(isOnline){
            case false:
            GameInitialization();
            FoodInitialization();
            break;

            case true:
            OnlineInitialization();
            if(player.isHost){
                FoodInitialization();
                snakeClient.SendRequest(gameOnline.FoodSerealization(food));
            }
            break;
        }
        
        
        //Console.WriteLine(isOnline);
        
        return true;
    }


    bool GameInitialization(){
       
        gameField.gameField = new char[gameField.width, gameField.height];
        for(int y = 0;y < gameField.height; y++){
            for(int x = 0; x < gameField.width; x++){
                gameField.gameField[x,y] = '-';
            }
        }

        snake.snake = new List<Point>();
       
        snake.snake.Add(new Point(1,2));
        snake.snake.Add(new Point(1,1));
        
        //lastTail = 4;
        snake.direction = Snake.Direction.Down;

        return true;
    }

    public bool OnlineInitialization(){
        isOnline = true;

        gameField.height =20;
        gameField.width = 30;
        gameField.gameField = new char[gameField.width, gameField.height];
        for(int y = 0;y < gameField.height; y++){
            for(int x = 0; x < gameField.width; x++){
                gameField.gameField[x,y] = '-';
            }
        }

        snake.snake = new List<Point>(){new Point(1,2), new Point(1,1)};
        snakeOpponent.snake = new List<Point>(){new Point(gameField.width-2, gameField.height-3), new Point(gameField.width-2, gameField.height-4)};
        
        snake.direction = Snake.Direction.Down;
        snakeOpponent.direction = Snake.Direction.Up;

        snakeClient = new SnakeClient();

        Console.WriteLine("awaiting 2 player");
        if(GetStartsParam(snakeClient.GetRequest())){
            return true;
        }

        return false;
    }


    bool GetStartsParam(string param){
        
        string[] paramArray = param.Split(',');


        if(paramArray[0] == "play"){
            if(paramArray[1] == "2"){
                player.isHost = false;
            }

        }
        

        return false;
    }

    bool isSnake = false;

    public bool FoodInitialization(){
        
        if(!isOnline){
            do{
                Point point = new Point(food.random.Next(0, gameField.width - 1), food.random.Next(0, gameField.height - 1));
                if(!snake.snake.Any(p => p.x == point.x && p.y == point.y)){
                    food.food = point;
                    gameField.gameField[food.food.x, food.food.y] = '*';
                    isSnake = false;
                }
                else{isSnake = true;}
            }while(isSnake);
        }
        
        else{
            do{
                Point point = new Point(food.random.Next(0, gameField.width - 1), food.random.Next(0, gameField.height - 1));
                if(!snake.snake.Any(p => p.x == point.x && p.y == point.y) && !snakeOpponent.snake.Any(p => p.x == point.x && p.y == point.y)){
                    food.food = point;
                    gameField.gameField[food.food.x, food.food.y] = '*';
                    isSnake = false;
                }
                else{isSnake = true;}

            }while(isSnake); 

        }
        
        return true;
    }

    public bool Eating(Point head){
        if(snake.snake[0].x == food.food.x && snake.snake[0].y == food.food.y){
            
            player.score++;
            delay -= player.score/2;
            snake.snake.Add(new Point(snake.temp2.x, snake.temp2.y));
            FoodInitialization();

            if(isOnline){
                snakeClient.SendRequest(gameOnline.FoodSerealization(food));
            }
            
            
        }
        return true;
    }

    string FoodSerealization(){
        return $"f,{food.food.x},{food.food.y}.";
    }

    void SnakeEated(ref Snake snake){
        snake.snake.Add(new Point(snake.temp2.x, snake.temp2.y));
    }
    bool OnlineEating(){
        if(player.isHost && snake.snake[0].x == food.food.x && snake.snake[0].y == food.food.y){

            SnakeEated(ref snake);
                       
            if(player.isHost){
                FoodInitialization();
                snakeClient.SendRequest(gameOnline.FoodSerealization(food));
            }
            player.score++;
            delay = 500 - player.score * 3;
            
        }else if(!player.isHost && snakeOpponent.snake[0].x == food.food.x && snakeOpponent.snake[0].y == food.food.y){
            SnakeEated(ref snakeOpponent);
            
            if(!player.isHost){
                FoodInitialization();
                snakeClient.SendRequest(gameOnline.FoodSerealization(food));
            }
            player.score++;
            delay = 500 - player.score * 3;
        }
        

        return true;
    }

    public bool SnakeMove(Snake snake){
        
        
        char snakeHead = '^';
        Snake.Direction where = snake.direction;
        Point position = snake.snake[0];

        switch(where){
            case Snake.Direction.Up:
            snake.nextPoint = new Point(position.x, position.y-1);
            snakeHead = '^';
            break;

            case Snake.Direction.Down:
            snake.nextPoint = new Point(position.x, position.y+1);
            snakeHead = 'v';
            break;

            case Snake.Direction.Left:
            snake.nextPoint = new Point(position.x-1, position.y+0);
            snakeHead = '<';
            break;

            case Snake.Direction.Right:
            snake.nextPoint = new Point(position.x+1, position.y+0);
            snakeHead = '>';
            break;
            }


        if(!isLoose(snake.nextPoint)){
            
            for(int i=0;i<snake.snake.Count;i++){
                    if(i == 0){
                        snake.temp = snake.snake[i];
                        snake.snake[i] = snake.nextPoint;
                    }
                    else{
                        snake.temp2 = snake.snake[i];
                        snake.snake[i] = snake.temp;
                        snake.temp = snake.temp2;
                    }
                    if(i == 0){
                        gameField.gameField[snake.snake[i].x,snake.snake[i].y] = snakeHead; 
                        
                    }else{
                        gameField.gameField[snake.snake[i].x,snake.snake[i].y] = '#';
                    }
                    
                    gameField.gameField[snake.temp2.x, snake.temp2.y] = '-';
                }
                return true;
        }
        else{

            return false;
        }

    }

    

     bool isLoose(Point nextPoint){
        if(nextPoint.x > gameField.width - 1 || nextPoint.y > gameField.height - 1 || snake.snake.Any(p => p.x == nextPoint.x && p.y == nextPoint.y || nextPoint.x < 0 || nextPoint.y < 0)){
            
            return true;
        }
        return false;
    }

    void SnakeTurn(Snake snake, ConsoleKey key){
        if(key == ConsoleKey.S && snake.direction != Snake.Direction.Up){
                        snake.direction = Snake.Direction.Down;
                    }
                    else if(key == ConsoleKey.D && snake.direction != Snake.Direction.Left){
                        snake.direction = Snake.Direction.Right;
                    }
                    else if(key == ConsoleKey.W && snake.direction != Snake.Direction.Down){
                        snake.direction = Snake.Direction.Up;
                    }
                    else if(key == ConsoleKey.A && snake.direction != Snake.Direction.Right){
                        snake.direction = Snake.Direction.Left;
                    }

                    
    }

    public async Task GameTick(){
        do{
            StartGame();
            
            while(play){
                var key = ConsoleKey.S;

                await Task.Delay(delay);
 
                
                if(Console.KeyAvailable){
                    key = Console.ReadKey(true).Key;
                    if(player.isHost || !isOnline)
                        SnakeTurn(snake, key);
                    else
                        SnakeTurn(snakeOpponent, key);
                    

                }
   
                

                if(isOnline){
                    if(player.isHost)
                    {
                        if(!SnakeMove(snake)){
                            play = false;

                            if(player.maxScore < player.score){
                                player.DBScoreSave();
                                player.maxScore = player.score;
                            }
                            snakeClient.SendRequest("l.");

                        }
                        else{
                            OnlineEating();

                            snakeClient.SendRequest(gameOnline.SnakeSerealization(snake));

                        }
                        
                    }
                    else{
                        if(!SnakeMove(snakeOpponent)){
                            play = false;

                            if(player.maxScore < player.score){
                                player.DBScoreSave();
                                player.maxScore = player.score;
                            }
                            snakeClient.SendRequest("l.");
                            consoleDrawing.LooseScreen();

                        }
                        else{
                            OnlineEating();

                            snakeClient.SendRequest(gameOnline.SnakeSerealization(snakeOpponent));
                        }
                        
                    }
                    
                }
                else{
                    if(!SnakeMove(snake)){
                        //play = false;

                        if(player.maxScore < player.score){
                            player.DBScoreSave();
                            player.maxScore = player.score;
                        }
                        break;
                    }
                    Eating(snake.snake[0]);
                }

                
                
                

                consoleDrawing.Drawing(gameField, player.score, player.maxScore);
            }
        }while(consoleDrawing.EndScreen(dataLayer,player));
        

        //return true;
    }

    

    // bool ScoreSaving(int score){
    //     string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "score.txt");
    //     using(FileStream snake = new FileStream(path, FileMode.OpenOrCreate)){
    //         byte[] newScore = System.Text.Encoding.Default.GetBytes(score.ToString());
    //         snake.Write(newScore, 0, newScore.Count());
    //     }

    //     return true;
    // }

    // int ScoreReading(){
    //     string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "score.txt");
    //     if (!File.Exists(path)) {
    //     return 0; 
    //     }
    //     using(FileStream readScore = File.OpenRead(path)){
    //         byte[] scoreBytes = new byte[readScore.Length];
    //         readScore.Read(scoreBytes, 0, scoreBytes.Length);
    //         int.TryParse(System.Text.Encoding.Default.GetString(scoreBytes), out maxScore);
    //     }
    //     return maxScore;
    // }
    bool Deserealization(string stream){
        if(stream == "null" || !play)
        {
            return false;
        }
        stream = stream[0..(stream.Length-1)];

        string[] fullStreamString = stream.Split('.');
        string[] stringStream;

        foreach (string mainStrings in fullStreamString)
        {
            stringStream = mainStrings.Split(',');


            if(stringStream[0] == "f"){
                food.food = new Point(int.Parse(stringStream[1]), int.Parse(stringStream[2]));
                gameField.gameField[food.food.x, food.food.y] = '*'; 

                snakeClient.SendRequest("null");
                consoleDrawing.Drawing(gameField, 1,1);
            }

            else if(stringStream[0] == "d"){
                if(player.isHost){
                    snakeOpponent.direction = Enum.Parse<Snake.Direction>(stringStream[1]); 
                }
                else{
                    snake.direction = Enum.Parse<Snake.Direction>(stringStream[1]);  
                }
                
            }
            else if(stringStream[0] == "l"){
                play = false;
                consoleDrawing.WinScreen();
            }
            else if(stringStream[0] == "s"){
                if(player.isHost){
                    snakeOpponent.direction = Enum.Parse<Snake.Direction>(stringStream[1]);
                    snakeOpponent.temp2 = new Point(int.Parse(stringStream[2]), int.Parse(stringStream[3]));
                    snakeOpponent.snake.Clear();
                    for (int i = 4; i < stringStream.Length; i+=2)
                    {
                        snakeOpponent.snake.Add(new Point(int.Parse(stringStream[i]), int.Parse(stringStream[i+1])));
                        gameField.gameField[int.Parse(stringStream[i]), int.Parse(stringStream[i+1])] = '#';
                    }
                    gameField.gameField[snakeOpponent.temp2.x, snakeOpponent.temp2.y] = '-';
                    
                    
                }
                else{
                    snake.direction = Enum.Parse<Snake.Direction>(stringStream[1]);
                    snake.temp2 = new Point(int.Parse(stringStream[2]), int.Parse(stringStream[3]));
                    snake.snake.Clear();
                    for (int i = 4; i < stringStream.Length; i+=2)
                    {
                        snake.snake.Add(new Point(int.Parse(stringStream[i]), int.Parse(stringStream[i+1])));
                        gameField.gameField[int.Parse(stringStream[i]), int.Parse(stringStream[i+1])] = '#';
                    }
                    gameField.gameField[snake.temp2.x, snake.temp2.y] = '-';
                }
                consoleDrawing.Drawing(gameField, 1,1);
            }
        }

        
        
        

        return true;
    }
    public async Task GetRequest(){
        if(isOnline){
            while(true){
                Deserealization(snakeClient.GetRequest());
                await Task.Delay(5);
            }
        }
        
    }
    
}

class ConsoleDrawing{

    public bool StartScreen(Player player, DataLayer dataLayer, ref bool isOnline){
        
        Console.Clear();
        

        string nick;
        do{
            Console.WriteLine("enter ur nickname: ");
            nick = Console.ReadLine();

        }while(!player.Login(nick));
        player.nickname = nick;
        player.DBScoreRead();
        

        Console.Clear();

        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("--START---");
        Console.WriteLine("---GAME---");
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("--PRESS---");
        Console.WriteLine("---'S'----");
        Console.WriteLine("ur max score: " + player.maxScore);
        Console.WriteLine("press 'X' to online");

        
        player.DBScoreRead();

        var key = Console.ReadKey(true).Key;
        if(key == ConsoleKey.S){
            return true;
        }
        else if(key == ConsoleKey.X){
            isOnline = true;
        }

        return false;
    }

    public bool EndScreen(DataLayer dataLayer, Player player){
        Console.Clear();

        do{
            Console.WriteLine("----------");
            Console.WriteLine("----------");
            Console.WriteLine("----------");
            Console.WriteLine("---GAME---");
            Console.WriteLine("---OVER---");
            Console.WriteLine("----------");
            Console.WriteLine("----------");
            Console.WriteLine("--AGAIN?--");
            Console.WriteLine("--PRESS---");
            Console.WriteLine("---'W'----");
            Console.WriteLine("score: " + player.score);
            Console.WriteLine("ur max score: " + player.maxScore);
            Console.WriteLine("\ntop scores: press 'Q'");

            var key = Console.ReadKey(true).Key;
            if(key == ConsoleKey.W || key == ConsoleKey.X){
                
                return true;
            }
            else if(key == ConsoleKey.Q){
                Console.Clear();
                Console.WriteLine(player.DBTopScoresRead());
                Console.ReadLine();
                Console.Clear();
            }
            else{
                Environment.Exit(0);
            }
        }while(true);
        

        //return false;
    }

    public bool LooseScreen(){
        Console.Clear();
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("---GAME---");
        Console.WriteLine("---OVER---");
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("---YOU----");
        Console.WriteLine("---LOST---");
        Console.WriteLine("----------");
        Environment.Exit(0);

        

        return true;
    }

    public bool WinScreen(){
        Console.Clear();
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("---GAME---");
        Console.WriteLine("---OVER---");
        Console.WriteLine("----------");
        Console.WriteLine("----------");
        Console.WriteLine("---YOU----");
        Console.WriteLine("---WON!---");
        Console.WriteLine("----------");
        Environment.Exit(0);


        return true;
    }

    public bool Drawing(GameField gameField, int score, int maxScore){
        StringBuilder stringBuilder = new StringBuilder();
        Console.Clear();

            for(int h = 0;h < gameField.height; h++){
                for(int w = 0; w < gameField.width; w++){
                    stringBuilder.Append(gameField.gameField[w,h]);
                }
                stringBuilder.AppendLine();
            }
            Console.WriteLine(stringBuilder.ToString());
            
            Console.WriteLine("score: "+score);
            Console.WriteLine("\nmax score: "+maxScore);
            return true;
    }
}
class GameField{
    
    public int height = 10;
    public int width = 10;

    public char[,] gameField;
}

class Program
{
    static async Task Main(string[] args)
    {
        Game game = new Game();
       

        await Task.WhenAll(game.GameTick(), game.GetRequest());
    }
}
    
