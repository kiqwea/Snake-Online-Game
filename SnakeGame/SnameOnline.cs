using System.Text;
using System.Data;


namespace MyProject.SnakeOnline{
    interface IOnlineGame{

        bool OnlineGetStartParam(string startsParam, Player player);
        string Serealization(Snake snake);
        string FoodSerealization(Food food);

    }
}