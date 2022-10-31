
using Dapper;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using System.Diagnostics.Metrics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

string Esymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
string Rsymbols = "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
Random r = new Random();
string connString = "Server=localhost; Database=postgres; User Id = postgres; Password = 12345" ;

string InvokeSumMed()
{
    NpgsqlConnection nc = new NpgsqlConnection(connString);
    nc.Open();
    string result = "";
    using (nc)
    {
        NpgsqlCommand cmd = new NpgsqlCommand("sum_med", nc);
        cmd.CommandType = CommandType.StoredProcedure;
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine("Сумма всех положительных чисел - " + reader["summ"].ToString());
            Console.WriteLine("Медиана всех дробных чисел - " + reader["med"].ToString());
        }
        return result;                
    }  
}


void InvokeImport()
{

    System.IO.StreamReader sr = new System.IO.StreamReader("one.txt", Encoding.Default);
    Console.WriteLine("Идет подсчет строк...");
    int amount = System.IO.File.ReadAllLines("one.txt").Length;
    
    int count = 0;
    while (!sr.EndOfStream)
    {
        NpgsqlConnection nc = new NpgsqlConnection(connString);
        nc.Open();
        string re = sr.ReadLine();
        string[] subs = re.Split("||");

        using (nc)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("call insert_data(:RDate, :REng, :RRus, :RInt, :RDouble)", nc);
            cmd.Parameters.AddWithValue("RDate", DbType.DateTime).Value = (DateTime.Parse(subs[0]));
            cmd.Parameters.AddWithValue("REng", DbType.String).Value =  subs[1];
            cmd.Parameters.AddWithValue("RRus", DbType.String).Value = subs[2];
            cmd.Parameters.AddWithValue("RInt", DbType.Int32).Value = Convert.ToInt32(subs[3]);
            cmd.Parameters.AddWithValue("RDouble", DbType.Double).Value = Convert.ToDouble(subs[4]);
            cmd.CommandType = CommandType.Text; 
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Добавлено строк - " + ++count);
        Console.WriteLine("Осталось строк - " + (amount - count));
    }
    
}

char GetRandomChar(string symbols) // случайный символ
{
    var index = r.Next(symbols.Length);
    return symbols[index];
}

string GetRandomArray(string symbols) //набор 10 случайных символов
{
    string array = "";
    for(int i = 0; i < 10; i++)
    {
        array += GetRandomChar(symbols);
    }
    return array;
}

String RandomDay() // случайная дата за 5 лет
{
    Random gen = new Random();
    DateTime start = new DateTime(DateTime.Today.Year-5, DateTime.Today.Month, DateTime.Today.Day);
    int range = (DateTime.Today - start).Days;
    return start.AddDays(gen.Next(range)).ToString("d");
}

void GenerateFiles() //сгенерировать файлы
{
    for (int i = 0; i < 100; i++)
    {
        string path = i + ".txt";
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            for (int j = 0; j < 100000; j++)
            {
                string text = RandomDay().ToString() + "||" + GetRandomArray(Esymbols) + "||" + GetRandomArray(Rsymbols) + "||" + r.Next(1, 100000) +
                    "||" + Math.Round(r.NextDouble() * (20 - 1) + 1, 8);

                writer.WriteLine(text);
            }
        }
        Console.WriteLine("Файл " + i + " создан и заполнен");
    }
    Console.WriteLine("Файлы созданы и заполнены");
}

void ManyToOne() //объеденить файлы в один
{
    Console.WriteLine("Введите подстроку для удаления"); 
    Console.WriteLine("Чтобы продолжить без удаления, нажмите Enter");
    string str = Console.ReadLine();
    bool flag = true;
    if (String.IsNullOrEmpty(str)) flag = false;
    System.IO.StreamWriter union = new System.IO.StreamWriter("one.txt", false);
    for (int i = 0; i < 100; i++)
    {
        System.IO.StreamReader sr = new System.IO.StreamReader(i + ".txt", Encoding.Default);
        
        System.IO.StreamWriter swtemp = new System.IO.StreamWriter("temp.txt", false);
        
        if (flag)
        {
            while (!sr.EndOfStream)
            {
                string re = sr.ReadLine();
                if (!re.Contains(str)) swtemp.WriteLine(re);
                else Console.WriteLine("Из файла " + i + " удалена строка ");
            }
            swtemp.Close();
            sr.Close();
            System.IO.StreamReader srtemp = new System.IO.StreamReader("temp.txt", Encoding.Default);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(i + ".txt", false);
            sw.Write(srtemp.ReadToEnd());
            sw.Close();
            srtemp.Close();
        }
        System.IO.StreamReader sr1 = new System.IO.StreamReader(i + ".txt", Encoding.Default);
        union.Write(sr1.ReadToEnd());
        
        sr.Close();
        swtemp.Close();
        Console.WriteLine("Файл " + i + " добавлен");
    }
    union.Close();
}

while (true)
{
    
    Console.WriteLine("Что вы хотите сделать?");
    Console.WriteLine("1. Сгенерировать файлы");
    Console.WriteLine("2. Импортировать файлы в БД");
    Console.WriteLine("3. Вывести сумму всех целых чисел и медиану дробных");
    Console.WriteLine("4. Объеденить файлы в один");
    Console.WriteLine("0. Выйти");

    int choice = Convert.ToInt32(Console.ReadLine());
    switch (choice)
    {
        case 0:
            Environment.Exit(0);
            break;
        case 1:
            GenerateFiles();
            break;
        case 2:
            InvokeImport();
            break;
        case 3:
            InvokeSumMed();
            break;
        case 4:
            ManyToOne();
            break;
        default: continue;
    }
}



