using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;

namespace TaskManagerApp
{
    class Program
    {
        static List<TaskItem> tasks = new List<TaskItem>();
        static TraceSource trace = new TraceSource("TaskManagerTrace");

        static void Main(string[] args)
        {
            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Программа запущена");
                bool running = true;

                while (running)
                {
                    Console.WriteLine("\n1. Добавить задачу\n2. Удалить задачу\n3. Показать список задач\n4. Выход");
                    Console.Write("Выбор: ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            AddTask();
                            break;
                        case "2":
                            RemoveTask();
                            break;
                        case "3":
                            ListTasks();
                            break;
                        case "4":
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Необработанная ошибка");
                Console.WriteLine("Произошла критическая ошибка. См. логи.");
            }
            finally
            {
                Log.Information("Программа завершается");
                trace.Close();
                Log.CloseAndFlush();
            }
        }

        static void AddTask()
        {
            trace.TraceEvent(TraceEventType.Start, 0, "Начало AddTask");
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                Console.Write("Введите название задачи: ");
                string title = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(title))
                    throw new ArgumentException("Название задачи не может быть пустым.");

                TaskItem task = new TaskItem { Id = Guid.NewGuid(), Title = title };
                tasks.Add(task);

                Log.Information("Добавлена задача {@Task}", task);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при добавлении задачи");
                Console.WriteLine("Ошибка при добавлении задачи. Подробнее см. логи.");
            }
            finally
            {
                sw.Stop();
                trace.TraceEvent(TraceEventType.Stop, 0, $"Завершено AddTask. Время: {sw.ElapsedMilliseconds} мс");
            }
        }

        static void RemoveTask()
        {
            trace.TraceEvent(TraceEventType.Start, 0, "Начало RemoveTask");
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                Console.Write("Введите название задачи для удаления: ");
                string title = Console.ReadLine();

                TaskItem task = tasks.Find(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

                if (task != null)
                {
                    tasks.Remove(task);
                    Log.Information("Задача \"{TaskTitle}\" удалена", task.Title);
                }
                else
                {
                    Log.Error("Ошибка: задача \"{TaskTitle}\" не найдена", title);
                    Console.WriteLine("Задача не найдена.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при удалении задачи");
                Console.WriteLine("Ошибка при удалении задачи. Подробнее см. логи.");
            }
            finally
            {
                sw.Stop();
                trace.TraceEvent(TraceEventType.Stop, 0, $"Завершено RemoveTask. Время: {sw.ElapsedMilliseconds} мс");
            }
        }

        static void ListTasks()
        {
            trace.TraceEvent(TraceEventType.Start, 0, "Начало ListTasks");
            Stopwatch sw = Stopwatch.StartNew();

            Console.WriteLine("\nСписок задач:");
            foreach (var task in tasks)
            {
                Console.WriteLine($"- {task.Id}: {task.Title}");
            }

            Log.Information("Показан список из {Count} задач", tasks.Count);

            sw.Stop();
            trace.TraceEvent(TraceEventType.Stop, 0, $"Завершено ListTasks. Время: {sw.ElapsedMilliseconds} мс");
        }
    }

    class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }
}
