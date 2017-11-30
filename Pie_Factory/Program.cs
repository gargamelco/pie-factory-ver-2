using System;
using System.Threading;
using System.Timers;

    /*
        TASK: 
        Pie Factory: pies are made from three components: filling, flavor and topping, each dispensed 
        from a respective hopper with one of these three ingredients.

        Robot Lucy:
        Adds the three ingredients to empty crusts that move on a conveyor belt.
        Can pause the conveyor belt if a ingredient is depleted.

        Robot Joe:
        Fills the hoppers with the respective ingredient.
        Makes sure hoppers are not overfull.
        Makes sure hoppers do not go empty.

        Lucy and Joe as separate threads. 

        Belt speed: one pie crust every 50 ms.
        One pie takes:
        250 gr filling.
        10 gr flavor.
        100 gr topping.
        Every dispensing takes 10 ms.
        Hoppers contain 2 kg material max.
        Each hopper is filled at speed 100 gr / 10 ms.
        Hopper filling start / stop happens immediately.

        Robot Lucy:
        1st – adds filling.
        2nd – adds flavor.
        3rd – adds topping.
        Pauses the conveyor belt, if a hopper does not contain enough ingredient for a successful dispense.
        Resumes the conveyor belt once the missing ingredient is available.

        Robot Joe:
        Fills one hopper at a time.
        Can fill a hopper only partially.

        Implement the factory as a C# program and test it
        Model the hoppers, the robots, and the conveyor belt
        Robots and the belt are serviced by separate threads
    */

namespace Pie_Factory
{
    class Program
    {
        //Definitions

        //autoresetevent for Lucy to pause the Conveyour Belt Thread
        static AutoResetEvent pauseBelt = new AutoResetEvent(false);

        //variables from the task
        const int beltSpeed = 5000;
        const int dispenseTime = 1000;
        const int hopperMaxAmount = 2000;
        const int hopperFillingSpeed = 1000;

        //starting amounts of the ingredients are random everytime
        static Random startingAmount = new Random();
        static int filling = startingAmount.Next(250, 2000);
        static int flavor = startingAmount.Next(10, 2000);
        static int topping = startingAmount.Next(100, 2000);

        static int pieCrust = 0;
        
        //some boolean vars to check speach and other
        static bool pauseConveyorBelt = false;
        static bool lucyWaitsStopTalk = false;
        static bool lucyPausedBeltStopTalk = false;

        private static object Lock = new object();

        //Robot Lucy speach is in red
        static void RobotLucy(object tag)
        {
            CancellationToken token = (CancellationToken)tag;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Robot Lucy has been cancelled.");
                    return;
                }

                //checking if we have enough amounts of the 3 ingredients for one pie
                if (filling < 250 || flavor < 10 || topping < 100)
                {
                    pauseConveyorBelt = true;  
                    pauseBelt.WaitOne();
                }
                else if (pieCrust < 1) //then checking if we have pie crust on the belt available
                {
                    if (!lucyWaitsStopTalk) //if Lucy said her speach once then she waits silently
                    {
                        pauseConveyorBelt = false;
                        pauseBelt.Set();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy is awaiting the next pie crust to appear on the conveyor belt.");
                        lucyWaitsStopTalk = true;
                    }
                    else
                    {
                        pauseConveyorBelt = false;
                        pauseBelt.Set();
                    }
                }
                //if everything is in tact then we proceed with dispensing the ingredients
                else
                {
                    lucyWaitsStopTalk = false;

                    lock (Lock)
                    {
                        filling -= 250;
                    }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy just dispensed 250 gr of filling for a pie - 1 sec to next dispense");
                        Thread.Sleep(1000);

                    lock (Lock)
                    {
                        flavor -= 10;
                    }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy just dispensed 10 gr of flavor for a pie - 1 sec to next dispense");
                        Thread.Sleep(1000);

                    lock (Lock)
                    {
                        topping -= 100;
                    }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy just dispensed 100 gr of topping for a pie - 1 sec to use the pie crust");
                        Thread.Sleep(1000);

                    lock (Lock)
                    {
                        pieCrust -= 1;
                    }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy just made one pie successfully!");
                    }
                }
            }
        

        static void RobotJoe(object tag)
        {
            CancellationToken token = (CancellationToken)tag;

            // variables for minimum amounts in the hopper before Joe to start to fill them
            int fillingMinAmount = 250;
            int flavorMinAmount = 10;
            int toppingMinAmount = 100;
            
            /* 
            variable which Joe will need to check his progress. 
            He sets for himself a goal everytime when there's to fill a hopper.
            It is a percentage.
            */
            int progressInPercentage;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Robot Joe has been cancelled.");
                    return;
                }

                if (pauseConveyorBelt)
                {
                    //checking minimum amounts in the hoppers
                    if (filling < fillingMinAmount)
                    {
                        //everytime his goal is different. This is to complete task's condition - filling a hopper only partially.
                        Random partialFillPercentage = new Random();
                        //it's between 50 and 99 %
                        int pfp = partialFillPercentage.Next(50, 99);

                        do
                        {
                            progressInPercentage = filling * 100 / hopperMaxAmount;
                            if (progressInPercentage < pfp)
                            {
                                Thread.Sleep(2000);
                                lock (Lock)
                                {
                                    filling += 100;
                                }
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Joe is filling the filling ingredient, because of it's insufficient amount. Filling + 100 gr");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Current amount of filling: {filling} gr");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"His progress at loading the filling is {progressInPercentage} % out of {pfp} % which Joe set for himself.");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Joe has reached his norm with filling the filling ingredient.");
                                break;
                            }

                        }
                        while (progressInPercentage < pfp);
                        pauseBelt.Set();
                    }

                    if (flavor < flavorMinAmount)
                    {
                        Random partialFillPercentage = new Random();
                        int pfp = partialFillPercentage.Next(50, 99);

                        do
                        {
                            progressInPercentage = flavor * 100 / hopperMaxAmount;
                            if (progressInPercentage < pfp)
                            {
                                Thread.Sleep(2000);
                                lock (Lock)
                                {
                                    flavor += 100;
                                }
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Joe is filling the flavor ingredient, because of it's insufficient amount. Flavor + 100 gr");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Current amount of flavor: {flavor} gr");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"His progress at loading the flavor is {progressInPercentage} % out of {pfp} % which Joe set for himself.");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Joe has reached his norm with filling the flavor ingredient.");
                                break;
                            }

                        }
                        while (progressInPercentage < pfp);
                        pauseBelt.Set();
                    }

                    if (topping < toppingMinAmount)
                    {
                        Random partialFillPercentage = new Random();
                        int pfp = partialFillPercentage.Next(50, 99);

                        do
                        {
                            progressInPercentage = topping * 100 / hopperMaxAmount;
                            if (progressInPercentage < pfp)
                            {
                                Thread.Sleep(2000);
                                lock (Lock)
                                {
                                    topping += 100;
                                }
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Joe is filling the topping ingredient, because of it's insufficient amount. Topping + 100 gr.");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Current amount of topping: {topping} gr");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"His progress at loading the topping is {progressInPercentage} % out of {pfp} % which Joe set for himself.");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Joe has reached his norm with filling the topping ingredient.");
                                break;
                            }
                        }
                        while (progressInPercentage < pfp);
                        pauseBelt.Set();
                    }
                }
            }
        }

        static void ConveyorBelt(object tag)
        {
            CancellationToken token = (CancellationToken)tag;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("ConveyorBelt has been cancelled.");
                    return;
                }

                //if conveyor belt is not paused then it releases its crusts every 5 seconds
                if (!pauseConveyorBelt)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("A new pie crust was released by the conveyor belt, next in 5 seconds");

                    lock (Lock)
                    {
                        pieCrust += 1;
                    }
                        lucyPausedBeltStopTalk = false;
                        Thread.Sleep(5000);
                    
                }
                //if its paused then this thread will wait.. until Lucy starts it
                else
                {
                    //this check is need to make sure Lucy will stop spamming that she paused the belt after she said it once
                    if (!lucyPausedBeltStopTalk)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Lucy paused temporarily the belt, because of the lack of sufficient amount of some of the ingredients");
                        lucyPausedBeltStopTalk = true;
                        pauseBelt.WaitOne();
                    }
                    else
                    {
                        pauseBelt.WaitOne();
                    }
                }
            }
        }

        //Method which will announce the current amount of ingredients at a time for our reference.
        private static void RevisionAnnouncer(object source, ElapsedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{Environment.NewLine} *** Current revision of the amounts of the ingredients at {DateTime.Now.ToString("h:mm:ss tt")}. ***");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" *** Filling: {filling} gr., Flavor {flavor} gr., Topping {topping} gr. ***");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" *** Next revision in 10 seconds. ***{Environment.NewLine}");
        }

        static void Main(string[] args)
        {
            //timer for our announcer
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(RevisionAnnouncer);
            timer.Interval = 10000;
            timer.Enabled = false;

            var belt = new Thread(ConveyorBelt);
            var lucy = new Thread(RobotLucy);
            var joe = new Thread(RobotJoe);

            var cts = new CancellationTokenSource();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" <<< To start The Pie Factory press S. To stop The Pie Factory press X. >>> {Environment.NewLine}");
            
            //we can start and stop our factory with key press
            while (true)
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.KeyChar == 'S')
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"{Environment.NewLine} *** Current revision of the amounts of the ingredients at {DateTime.Now.ToString("h:mm:ss tt")}. ***");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($" *** Filling: {filling} gr., Flavor {flavor} gr., Topping {topping} gr. ***");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($" *** Next revision in 10 seconds. ***{Environment.NewLine}");

                            timer.Enabled = true;

                            belt.Start(cts.Token);
                            lucy.Start(cts.Token);
                            joe.Start(cts.Token);
                        }
                        else if (key.KeyChar == 'X')
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"{Environment.NewLine} <<< Please wait until all threads are done with their jobs and are cancelled safely! >>> {Environment.NewLine}");

                            timer.Enabled = false;
                            cts.Cancel();

                            belt.Join();
                            lucy.Join();
                            joe.Join();

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"{Environment.NewLine} <<< All threads have been successfully cancelled! >>> {Environment.NewLine}");
                        }
                    }
                }
                catch (ThreadStateException e)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{Environment.NewLine} <<< The program has been terminated, please restart in order to execute again! >>> {Environment.NewLine}");
                }
             }
         }
     }
}
    

