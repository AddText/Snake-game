﻿using System;
using Game.Classes;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Security.Principal;

static class GameHandler
{
    static Random rnd = new Random();

    static Window window = new Window("Game", ConsoleColor.Red, ConsoleColor.White);

    static Snake snake = new Snake(new LinkedListNode<Point>(new Point(20, 20)), new LinkedList<Point>(), window,
        ConsoleColor.Black, ConsoleColor.White);

    static Food food = new Food(ConsoleColor.Black, new Point(rnd.Next(window.MaxPoint.X + 1, window.MinPoint.X - 1),
        rnd.Next(window.MaxPoint.Y + 1, window.MinPoint.Y - 1)));

    static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    static bool alive = true;

    static void Main(string[] args)
    {
        window.ConsoleResizeDisable();
        window.TextResizeEnable();
        MainMenu();
    }
    static ConsoleKey GetValidkey(ConsoleKey[] validKeys)
    {
        ConsoleKey key = new ConsoleKey();
        while (!validKeys.Contains(key))
        {
            key = Console.ReadKey(true).Key;
        }
        return key;
    }
    static void ResetGame()
    {
        Console.Clear();
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Token.WaitHandle.WaitOne();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = new CancellationTokenSource();
        Window.GameOver.Clear();
        snake.Body.Clear();
        snake.Head.Value = new Point(20, 20);
        snake.ResetDirection();
        snake.eating = false;
        alive = true;
        food.FoodExist = false;
    }
    static void MainMenu()
    {
        ResetGame();
        window.ResizeAndPrintCenteredText(Menu.Title, cancellationTokenSource);
        ConsoleKey key = GetValidkey(new ConsoleKey[]  { ConsoleKey.Escape, ConsoleKey.Enter });
        if (key.Equals(ConsoleKey.Enter))
        {
            window.TextResizeDisable();
            window.ConsoleResizeEnable();
            ResetGame();
            Start();
        }
        if (key.Equals(ConsoleKey.Escape))
        {
            Environment.Exit(0);
        }
    }

    static void Start()
    {
        Console.Clear();
        window.CalculateFrame();
        window.DrawFrame();

        if(cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Token.WaitHandle.WaitOne();
            cancellationTokenSource.Dispose();
        }
        cancellationTokenSource = new CancellationTokenSource();

        Task.Run(() =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                window.ConsoleResize();
            }
        }, cancellationTokenSource.Token);
        snake.BodyInit(5);
        GetFoodPosition();
        Update();
        Console.ReadKey();
    }

    static void Update()
    {
        alive = true;
        while (alive)
        {
            snake.Move();
            snake.BodyMove();
            OnCollisionEnter();
            Thread.Sleep(75);
        }
    }

    static void GetFoodPosition()
    {
        food.FoodExist = false;
        food.Position = new Point(rnd.Next(window.MaxPoint.X + 1, window.MinPoint.X - 1),
        rnd.Next(window.MaxPoint.Y + 1, window.MinPoint.Y - 1));

        foreach (var item in snake.Body)
        {
            if (food.Position.Equals(item))
            {
                GetFoodPosition();
            }
            food.Draw();
            food.FoodExist = true;
        }
    }

    static void OnCollisionEnter()
    {
        if(snake.Head.Value.X.Equals(window.MinPoint.X))
        {
            Console.SetCursorPosition(snake.Head.Value.X, snake.Head.Value.Y);
            Console.WriteLine("║");
            snake.Head.Value = new Point(window.MaxPoint.X + 1, snake.Head.Value.Y);
        }
        if(snake.Head.Value.X.Equals(window.MaxPoint.X))
        {
            Console.SetCursorPosition(snake.Head.Value.X, snake.Head.Value.Y);
            Console.WriteLine("║");
            snake.Head.Value = new Point(window.MinPoint.X - 1, snake.Head.Value.Y);
        }
        if(snake.Head.Value.Y.Equals(window.MinPoint.Y))
        {
            Console.SetCursorPosition(snake.Head.Value.X, snake.Head.Value.Y);
            Console.WriteLine("═");
            snake.Head.Value = new Point(snake.Head.Value.X, window.MaxPoint.Y + 1);
        }
        if(snake.Head.Value.Y.Equals(window.MaxPoint.Y))
        {
            Console.SetCursorPosition(snake.Head.Value.X, snake.Head.Value.Y);
            Console.WriteLine("═");
            snake.Head.Value = new Point(snake.Head.Value.X, window.MinPoint.Y - 1);
        }
        if (snake.Head.Value.Equals(food.Position))
        { 
        
            snake.eating = true;
            Console.SetCursorPosition(snake.Head.Value.X, snake.Head.Value.Y);
            Console.WriteLine(" ");
            GetFoodPosition();
            food.FoodExist = false;
        }
        foreach(var itemn in snake.Body)
        {
            if (snake.Head.Value.Equals(itemn))
            {
                alive = false;
                Task.Run(() => snake.DeathAnimation(), cancellationTokenSource.Token);
                Task.Run(() =>
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    window.ResizeAndPrintCenteredText(Menu.GameOver, cancellationTokenSource);
                }, cancellationTokenSource.Token);
                ConsoleKey key = GetValidkey(new ConsoleKey[] { ConsoleKey.Escape, ConsoleKey.Enter });
                if (key.Equals(ConsoleKey.Enter))
                {
                    MainMenu();
                    Start();
                }
                if (key.Equals(ConsoleKey.Escape))
                {
                    Environment.Exit(0);
                }
                break;
            }
        }
    }
}
