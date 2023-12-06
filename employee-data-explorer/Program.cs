using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

class Program
{
    private const string XmlFilePath = "employees.xml";

    public static void Main()
    {
        ShowMenu("main");
    }

    private static void ShowMenu(string menuType)
    {
        while (true)
        {
            switch (menuType)
            {
                case "main":
                    Console.WriteLine("1. Создать XML файл");
                    Console.WriteLine("2. Добавить/Обновить данные о сотруднике");
                    Console.WriteLine("3. Поиск сотрудников по фамилии");
                    Console.WriteLine("4. Найти все должности и всех рабочих сотрудников в каждом отеделе");
                    Console.WriteLine("5. Сотрудники которые работают более чем в 1 отделе");
                    Console.WriteLine("6. Отделы с не более чем 3 сотрудниками");
                    Console.WriteLine("7. Топ годов по увольнению/найму");
                    Console.WriteLine("8. Вывод сотрудников у которых юбилей");
                    Console.WriteLine("9. Экспорт данных в XML");
                    Console.WriteLine("10. График изменения курса валют");
                    Console.WriteLine("11. Выйти");
                    Console.Write("Введите ваш выбор: ");

                    var choice = int.Parse(Console.ReadLine()!);

                    switch (choice)
                    {
                        case 1:
                            CreateXmlFile();
                            break;
                        case 2:
                            AddOrUpdateEmployeeData();
                            break;
                        case 3:
                            SearchEmployeesBySurname();
                            break;
                        case 4:
                            FindActiveEmployees();
                            break;
                        case 5:
                            FindEmployeeMultipleDepart();
                            break;
                        case 6:
                            FindSmallDeparts();
                            break;
                        case 7:
                            TopYears();
                            break;
                        case 8:
                            BigBirthdays();
                            break;
                        case 9:
                            ExportDataToXml();
                            break;
                        case 11:
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Некорректный выбор, пожалуйста попробуйте ещё раз");
                            break;
                    }

                    break;
                case "confirmClose":
                    Console.WriteLine("Выйти из программы ?");
                    Console.WriteLine("1. Да");
                    Console.WriteLine("2. Нет");

                    var choice2 = int.Parse(Console.ReadLine()!);

                    switch (choice2)
                    {
                        case 1:
                            Environment.Exit(0);
                            break;
                        case 2:
                            menuType = "main";
                            continue;
                    }
                    break;
                case "sortEmployeeData":
                    Console.WriteLine("Как отображать данные ?");
                    Console.WriteLine("1. Сначала старые");
                    Console.WriteLine("2. Сначала новые");
                    var choice3 = int.Parse(Console.ReadLine()!);

                    switch (choice3)
                    {
                        case 1:
                            SearchEmployeesBySurname();
                            break;
                        case 2:
                            SearchEmployeesBySurname(false);
                            break;
                    }
                    break;
                case "showSalaryPayments":
                    Console.WriteLine("Перейти к выводу зачислений заработной платы ?");
                    Console.WriteLine("1. Да");
                    Console.WriteLine("2. Выйти в главное меню");
                    var choice4 = int.Parse(Console.ReadLine()!);

                    switch (choice4)
                    {
                        case 1:
                            DisplaySalaryPayments();
                            break;
                        case 2:
                            menuType = "main";
                            continue;
                    }
                    break;
            }

            break;
        }
    }

    private static void CreateXmlFile()
    {
        var employees = new XElement("Сотрудники");
        employees.Save(XmlFilePath);
        Console.WriteLine("XML файл успешно создан.");
        ShowMenu("confirmClose");
    }

    private static void AddOrUpdateEmployeeData()
    {
        var xdoc = XDocument.Load(XmlFilePath);

        Console.Write("Введите полное имя сотрудника: ");
        var fullName = Console.ReadLine()!;
        
        var existingEmployee = xdoc.Descendants("Сотрудник")
            .FirstOrDefault(e => e.Element("ФИО").Value == fullName);

        if (existingEmployee != null)
        {
            var job = ResolveJob();
            var jobXml = new XElement("Работа",
                    new XElement("Название_должности", job.Item1),
                    new XElement("Дата_начала", job.Item2.ToString("dd.MM.yyyy")),
                    new XElement("Дата_окончания", job.Item8? "-" : job.Item3.ToString("dd.MM.yyyy")),
                    new XElement("Отдел", job.Item4)
                );
            var salaryXml = new XElement("Список_зарплат",
                new XElement("Зарплата",
                    new XElement("Год", job.Item7),
                    new XElement("Месяц", job.Item6),
                    new XElement("Итого", job.Item6 * job.Item5)
                )
            );
            existingEmployee.Element("Список_работ")!.Add(jobXml);
            existingEmployee.Element("Список_зарплат")!.Add(salaryXml);
            xdoc.Save(XmlFilePath);
            Console.WriteLine("Данные о сотруднике успешно обновлены.");
            ShowMenu("confirmClose");
        }
        else
        {
            Console.Write("Введите год рождения сотрудника: ");
            var birthdate = int.Parse(Console.ReadLine()!);
            var job = ResolveJob();
            
            var newEmployee = new XElement("Сотрудник",
                new XElement("ФИО", fullName),
                new XElement("Год_рождения", birthdate),
                new XElement("Список_работ",
                    new XElement("Работа",
                        new XElement("Название_должности", job.Item1),
                        new XElement("Дата_начала", job.Item2.ToString("dd.MM.yyyy")),
                        new XElement("Дата_окончания", job.Item8? "-" : job.Item3.ToString("dd.MM.yyyy")),
                        new XElement("Отдел", job.Item4)
                    )
                ),
                new XElement("Список_зарплат",
                    new XElement("Зарплата",
                        new XElement("Год", job.Item7),
                        new XElement("Месяц", job.Item6),
                        new XElement("Итого", job.Item6 * job.Item5)
                    )
                )
            );
                xdoc.Element("Сотрудники")!.Add(newEmployee);
            xdoc.Save(XmlFilePath);
            Console.WriteLine("Сотрудник добавлен успешно.");
            ShowMenu("confirmClose");
        }
    }

    private static (string, DateTime, DateTime, string, int, int, int, bool) ResolveJob()
    {
        Console.Write("Напишите название занимаемой должности: ");
        var jobTitle = Console.ReadLine()!;
        Console.Write("Напишите дату вступления в долженость в (dd.MM.yyy): ");
        var startTime = Console.ReadLine()!;
        var startDate = DateTime.ParseExact(startTime, "dd.MM.yyyy", null);
        dynamic jobEnd = "-";
        var isChecked = "not";
        while (isChecked == "not")
        {
            Console.Write("Это его текущая должность ? Напечатайте y или n: ");
            var answer = Console.ReadLine();
            switch (answer)
            {
                case "n":
                {
                    Console.Write("Напишите дату оконччания работы в (dd.MM.yyyy): ");
                    var endData = Console.ReadLine()!;
                    jobEnd = DateTime.ParseExact(endData, "dd.MM.yyyy", null);
                    isChecked = "yes";
                    break;
                }
                case "y":
                    Console.Write("Работник ещё в должности.");
                    isChecked = "yes";
                    break;
                default:
                    Console.Write("Попробуйте ещё раз");
                    break;
            }
        }
        Console.Write("Напишите название отдела: ");
        var departmentName = Console.ReadLine()!;
        Console.Write("Напишите зарплату в месяц: ");
        var salaryMonth = int.Parse(Console.ReadLine()!);

        var isActive = false;
        var endDate = DateTime.Now;
        if (jobEnd is DateTime) endDate = jobEnd;
        else isActive = true;
        var totalDays = (endDate - startDate).TotalDays;
        var elapsedMonth = (int)(totalDays / 30.44);
        var salaryYear = 0;
        if (elapsedMonth <= 12) salaryYear = salaryMonth * elapsedMonth;
        else salaryYear = salaryMonth * 12;
        return (jobTitle, startDate, endDate, departmentName, elapsedMonth, salaryMonth, salaryYear, isActive);
    }

    static void SearchEmployeesBySurname(bool oldFirst=true)
    {
        XDocument xdoc = XDocument.Load(XmlFilePath);

        Console.Write("Введите фамилию сотрудника для поиска: ");
        string surname = Console.ReadLine()!;

        var matchingEmployees = xdoc.Descendants("Сотрудник")
            .Where(e => e.Element("ФИО")!.Value.StartsWith(surname))
            .ToList();

        if (matchingEmployees.Any())
        {
            foreach (var employee in matchingEmployees)
            {
                Console.WriteLine($"Сотрудник: {employee.Element("ФИО")!.Value}");
                var jobs = employee.Element("Список_работ")!;
                if (oldFirst)
                {
                    foreach (var job in jobs.Elements("Работа"))
                    {
                        Console.WriteLine($"Должность: {job.Element("Название_должности")!.Value}");
                        Console.WriteLine($"Дата вступления в должность: {job.Element("Дата_начала")!.Value}");
                        var fireDate = job.Element("Дата_окончания")!.Value;
                        if (fireDate == "-")
                        {
                            Console.WriteLine("Дата увольнения: текущая работа");
                        }
                        else
                        {
                            Console.WriteLine($"Дата увольнения {fireDate}");
                        }
                        Console.WriteLine($"Отдел: {job.Element("Отдел")!.Value}");
                    }
                }
                else
                {
                    var validDataArr = new List<string>();
                    foreach (var job in jobs.Elements("Работа"))
                    {
                        validDataArr.Add($"Должность: {job.Element("Название_должности")!.Value}");
                        validDataArr.Add($"Дата вступления в должность: {job.Element("Дата_начала")!.Value}");
                        var fireDate = job.Element("Дата_окончания")!.Value;
                        if (fireDate == "-")
                        {
                            validDataArr.Add("Дата увольнения: текущая работа");
                        }
                        else
                        {
                            validDataArr.Add($"Дата увольнения {fireDate}");
                        }
                        validDataArr.Add($"Отдел: {job.Element("Отдел")!.Value}");
                    }

                    validDataArr.Reverse();

                    foreach (var jobInfo in validDataArr)
                    {
                        Console.WriteLine(jobInfo);
                    }
                }
            }
            ShowMenu("showSalaryPayments");
        }
        else
        {
            Console.WriteLine("Работников с такой фамилией не найдено");
        }
    }

    static void DisplaySalaryPayments()
    {
        Console.WriteLine("Уточните полное имя сотрудника: ");
        var eFullName = Console.ReadLine()!; 
        var xdoc = XDocument.Load(XmlFilePath);

        Console.Write("Введите дату начала (MM.yyyy): ");
        var startDateString = Console.ReadLine();
        var startDate = DateTime.ParseExact(startDateString, "MM.yyyy", null);

        Console.Write("Введите дату конца (MM.yyyy): ");
        var endDateString = Console.ReadLine();
        var endDate = DateTime.ParseExact(endDateString, "MM.yyyy", null);
        
        var existingEmployee = xdoc.Descendants("Сотрудник")
            .FirstOrDefault(e => e.Element("ФИО").Value == eFullName);
        if (existingEmployee != null)
        {
            var salaryPayments = existingEmployee.Descendants("Зарплата")
                .Where(s =>
                {
                    var paymentDate = DateTime.ParseExact(s.Element("Год")!.Value + "-" + s.Element("Месяц")!.Value,
                        "MM.yyyy", null);
                    return paymentDate >= startDate && paymentDate <= endDate;
                })
                .ToList();

            if (salaryPayments.Any())
            {
                foreach (var payment in salaryPayments)
                {
                    Console.WriteLine(
                        $"Год: {payment.Element("Год")!.Value}, Месяц: {payment.Element("Месяц")!.Value}, Общая зарплата: {payment.Element("Итого")!.Value}");
                }
            }
            else
            {
                Console.WriteLine("Выплат зарплат не найдено за это этот период.");
                ShowMenu("confirmClose");
            }
        }
        else
        {
            Console.WriteLine("Сотрудник не найден");
            ShowMenu("confirmClose");
        }
    }

    private static void FindActiveEmployees()
    {
        var xdoc = XDocument.Load(XmlFilePath);
        var activeEmployeesCount = xdoc.Descendants("Сотрудник")
            .Count(e =>
            {
                var endDateElement = e.Element("Список_работ")?.Element("Работа")?.Element("Дата_окончания");
                return endDateElement == null || endDateElement.Value == "-";
            });
        Console.WriteLine($"Всего текущих работников: {activeEmployeesCount}");
        

        // Получение уникальных отделов и сотрудников для каждого отдела
        var departments = from employee in xdoc.Descendants("Сотрудник")
            from work in employee.Descendants("Работа")
            where work.Element("Дата_окончания")?.Value == "-"
            group new
            {
                ФИО = employee.Element("ФИО")?.Value,
                Должность = work.Element("Название_должности")?.Value
            } by work.Element("Отдел")?.Value into departmentGroup
            select new
            {
                Отдел = departmentGroup.Key,
                Сотрудники = departmentGroup.ToList()
            };

        // Вывод информации по каждому отделу
        foreach (var department in departments)
        {
            Console.WriteLine($"Отдел: {department.Отдел}");

            // Список уникальных должностей для отдела
            var uniquePositions = department.Сотрудники.Select(s => s.Должность).Distinct().ToList();

            Console.WriteLine($"Должности: {string.Join(", ", uniquePositions)}");

            // Вывод количества работающих сотрудников в отделе
            Console.WriteLine($"Количество работающих сотрудников: {department.Сотрудники.Count}");

            // Вычисление и вывод доли работающих сотрудников
            var totalDepartmentEmployees = xdoc
                .Descendants("Сотрудник")
                .Count(e => e.Descendants("Работа")
                    .Any(r => r.Element("Отдел")?.Value == department.Отдел &&
                              r.Element("Дата_окончания")?.Value == "-"));
            var percentage = (department.Сотрудники.Count / totalDepartmentEmployees) * 100;

            Console.WriteLine($"Доля работающих сотрудников: {percentage:F2}%\n");

        }

        ShowMenu("confirmClose");
    }

    private static void FindEmployeeMultipleDepart()
    {
        var xdoc = XDocument.Load(XmlFilePath);
        var employees = from employee in xdoc.Descendants("Сотрудник")
            where employee.Descendants("Работа").Count() > 1
            select new
            {
                ФИО = employee.Element("ФИО")?.Value,
                СписокРабот = employee.Descendants("Работа").Select(r => r.Element("Отдел")?.Value).ToList()
            };

        // Вывод результатов
        foreach (var employee in employees)
        {
            Console.WriteLine($"ФИО: {employee.ФИО}");
            Console.WriteLine("Отделы:");
            foreach (var department in employee.СписокРабот)
            {
                Console.WriteLine($"  - {department}");
            }
            Console.WriteLine();
        }
        ShowMenu("confirmClose");
    }

    private static void FindSmallDeparts()
    {
        XDocument xdoc = XDocument.Load(XmlFilePath);
        var departmentsWithFewEmployees = xdoc.Descendants("Сотрудник")
            .SelectMany(s => s.Descendants("Отдел").Select(o => o.Value))
            .GroupBy(d => d)
            .Where(g => g.Count() <= 3)
            .Select(g => g.Key)
            .ToList();


        Console.WriteLine($"Отделы где работают не более 3 человек: {string.Join(", ", departmentsWithFewEmployees)}");
        ShowMenu("confirmClose");
    }

    private static void TopYears()
    {
        var xdoc = XDocument.Load(XmlFilePath);
        var hiringYears = xdoc.Descendants("Сотрудник")
            .SelectMany(e => e.Descendants("Работа"))
            .Where(r => r.Element("Дата_начала") != null)
            .Select(r => DateTime.ParseExact(r.Element("Дата_начала").Value, "dd.MM.yyyy", CultureInfo.InvariantCulture).Year)
            .ToList();

        var firingYears = xdoc.Descendants("Сотрудник")
            .SelectMany(e => e.Descendants("Работа"))
            .Where(r => r.Element("Дата_окончания") != null && r.Element("Дата_окончания").Value != "-")
            .Select(r => DateTime.ParseExact(r.Element("Дата_окончания").Value, "dd.MM.yyyy", CultureInfo.InvariantCulture).Year)
            .ToList();

        if (hiringYears.Any())
        {
            int maxHiringYear = hiringYears.GroupBy(y => y).OrderByDescending(g => g.Count()).First().Key;
            int minHiringYear = hiringYears.GroupBy(y => y).OrderBy(g => g.Count()).First().Key;
            Console.WriteLine($"Год с наибольшим количеством нанятых сотрудников: {maxHiringYear}");
            Console.WriteLine($"Год с наименьшим количеством нанятых сотрудников: {minHiringYear}");
        }
        else
        {
            Console.WriteLine("Нет данных о найме сотрудников.");
        }

        if (firingYears.Any())
        {
            int maxFiringYear = firingYears.GroupBy(y => y).OrderByDescending(g => g.Count()).First().Key;
            int minFiringYear = firingYears.GroupBy(y => y).OrderBy(g => g.Count()).First().Key;
            Console.WriteLine($"Год с наибольшим количеством увольнений: {maxFiringYear}");
            Console.WriteLine($"Год с наименьшим количеством увольнений: {minFiringYear}");
        }
        else
        {
            Console.WriteLine("Нет данных об увольнении сотрудников.");
        }
        ShowMenu("confirmClose");
    }

    private static void BigBirthdays()
    {
        var xdoc = XDocument.Load(XmlFilePath);
        var employeesWithAnniversary = xdoc.Descendants("Сотрудник")
            .Select(e => new
            {
                ФИО = e.Element("ФИО")?.Value,
                ГодРождения = int.Parse(e.Element("Год_рождения")?.Value),
                Возраст = DateTime.Now.Year - int.Parse(e.Element("Год_рождения")?.Value)
            })
            .Where(e => e.Возраст % 5 == 0)
            .ToList();

        foreach (var employee in employeesWithAnniversary)
        {
            Console.WriteLine($"В этом году сотруднику: {employee.ФИО}, исполняется: {employee.Возраст} лет");
        }
        ShowMenu("confirmClose");
    }
    

    static void ExportDataToXml()
    {
        var xdoc = XDocument.Load(XmlFilePath);

        var departments = xdoc.Descendants("Работа")
            .Where(r => r.Element("Дата_окончания")?.Value == "-")
            .GroupBy(r => r.Element("Отдел")?.Value)
            .Select(group => new
            {
                Название = group.Key,
                Количество_работающих_сотрудников = group.Count(),
                Количество_работающих_сотрудников_молодежь = group.Count(e =>
                {
                    var value = e.Parent.Element("Список_работ")?.Parent
                        ?.Element("Год_рождения")?.Value;
                    return value != null &&
                           e.Parent?.Element("Список_работ").Parent?.Element("Год_рождения") != null &&
                           !string.IsNullOrEmpty(e.Parent?.Element("Список_работ").Parent.Element("Год_рождения")
                               .Value) &&
                           (DateTime.Now.Year - int.Parse(value)) < 30;
                })
            })
            .ToList();

        // Создание XML-структуры
        var xmlResult = new XElement("Отделы",
            departments.Select(dep => new XElement("Отдел",
                new XAttribute("Название", dep.Название),
                new XElement("Количество_работающих_сотрудников", dep.Количество_работающих_сотрудников),
                new XElement("Количество_работающих_сотрудников_молодежь", dep.Количество_работающих_сотрудников_молодежь)
            ))
        );

        // Сохранение XML в файл
        xmlResult.Save("export.xml"); // Замените на фактический путь для сохранения

        Console.WriteLine("Экспорт данных выполнен успешно.");
        ShowMenu("confirmClose");
    }

    static void FetchAndDisplayCurrencyRates()
    {
        // Implement logic to fetch and display currency rates
    }
}
