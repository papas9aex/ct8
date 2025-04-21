using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;

namespace FullTaskManager
{
    class Program
    {
        static List<TaskItem> tasks = new List<TaskItem>();
        static TraceSource trace = new TraceSource("TaskManagerTrace");

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                bool running = true;
                while (running)
                {
                    Console.WriteLine("Введите команду: Add / Remove / List / Exit");
                    string command = Console.ReadLine();
                    switch (command?.ToLower())
                    {
                        case "add":
                            TraceAndExecute("Add", AddTask);
                            break;
                        case "remove":
                            TraceAndExecute("Remove", RemoveTask);
                            break;
                        case "list":
                            TraceAndExecute("List", ListTasks);
                            break;
                        case "exit":
                            Log.Information("Программа завершается");
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Неизвестная команда.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Необработанная ошибка");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static void TraceAndExecute(string operationName, Action operation)
        {
            trace.TraceEvent(TraceEventType.Start, 0, $"Начало операции: {operationName}");
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                operation.Invoke();
                trace.TraceEvent(TraceEventType.Stop, 0, $"Операция {operationName} завершена. Время: {sw.ElapsedMilliseconds} мс");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при выполнении операции {Operation}", operationName);
                Console.WriteLine("Произошла ошибка. Подробнее см. логи.");
            }
            finally
            {
                sw.Stop();
            }
        }

        static void AddTask()
        {
            Console.Write("Введите название задачи: ");
            string title = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название задачи не может быть пустым");

            Console.Write("Введите описание задачи: ");
            string desc = Console.ReadLine();

            Console.Write("Введите приоритет задачи: ");
            string prio = Console.ReadLine();

            var task = new TaskItem { Title = title, Description = desc, Priority = prio };
            tasks.Add(task);

            Log.Information("Добавлена задача {@Task}", task);
        }

        static void RemoveTask()
        {
            Console.Write("Введите название задачи для удаления: ");
            string title = Console.ReadLine();

            var task = tasks.Find(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (task != null)
            {
                tasks.Remove(task);
                Log.Information("Задача "{Title}" удалена", title);
            }
            else
            {
                Log.Warning("Ошибка: задача "{Title}" не найдена", title);
                throw new Exception($"Задача "{title}" не найдена");
            }
        }

        static void ListTasks()
        {
            Console.WriteLine("Список задач:");
            foreach (var task in tasks)
            {
                Console.WriteLine($"- {task.Title} | {task.Description} | {task.Priority}");
            }
            Log.Information("Показан список из {Count} задач", tasks.Count);
        }

        class TaskItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Priority { get; set; }
        }
    }
}
