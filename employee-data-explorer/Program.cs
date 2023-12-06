using System;
using System.Collections.Generic;
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
                    Console.WriteLine("4. Сотрудники которые работают более чем в 1 отделе");
                    Console.WriteLine("5. Отделы с не более чем 1 сотрудником");
                    Console.WriteLine("6. Топ годов по увольнению/найму");
                    Console.WriteLine("7. Вывод сотрудников у которых юбилей");
                    Console.WriteLine("8. Экспорт данных в XML");
                    Console.WriteLine("9. График изменения курса валют");
                    Console.Write("Enter your choice: ");

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
                            DisplaySalaryPayments();
                            break;
                        case 5:
                            PerformDataAnalysis();
                            break;
                        case 6:
                            ExportDataToXml();
                            break;
                        case 7:
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
            }

            break;
        }
    }

    private static void CreateXmlFile()
    {
        var employees = new XElement("Сотрудники");
        employees.Save(XmlFilePath);
        Console.WriteLine("XML файл успешно создан.");
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
            Console.Write("Введите название новой должности: ");
            var jobTitle = Console.ReadLine()!;

            existingEmployee.Element("Работа")!.Value = jobTitle;

            xdoc.Save(XmlFilePath);
            Console.WriteLine("Employee data updated successfully.");
        }
        else
        {
            Console.Write("Введите год рождения сотрудника");
            var birthdate = int.Parse(Console.ReadLine()!);
            Console.Write("Напишите название занимаемой должности");
            var jobTitle = Console.ReadLine()!;
            Console.Write("Напишите дату вступления в долженость в (DD-MM-YYYY)");
            var startTime = Console.ReadLine()!;
            var startDate = DateTime.ParseExact(startTime, "DD-MM-YYYY", null);
            dynamic jobEnd = "-";
            var isChecked = "not";
            while (isChecked == "not")
            {
                Console.Write("Это его текущая должность ? Напечатайте y или n");
                var answer = Console.ReadLine();
                switch (answer)
                {
                    case "n":
                    {
                        Console.Write("Напишите дату оконччания работы в (DD-MM-YYYY)");
                        var endData = Console.ReadLine()!;
                        jobEnd = DateTime.ParseExact(endData, "DD-MM-YYYY", null);
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
            Console.Write("Напишите название отдела");
            var departmentName = Console.ReadLine()!;
            Console.Write("Напишите зарплату в месяц");
            var salaryMonth = int.Parse(Console.ReadLine()!);

            var endDate = DateTime.Now;
            if (jobEnd != "-") endDate = jobEnd;
            var totalDays = (endDate - startDate).TotalDays;
            var elapsedMonth = (int)(totalDays / 30.44);
            var salaryYear = 0;
            if (elapsedMonth <= 12) salaryYear = salaryMonth * elapsedMonth;
            else salaryYear = salaryMonth * 12;
            
            
            var newEmployee = new XElement("Сотрудник",
                new XElement("ФИО", fullName),
                new XElement("Год_рождения", birthdate),
                new XElement("Список_работ",
                    new XElement("Работа",
                        new XElement("Название_должности", jobTitle),
                        new XElement("Дата_начала", startDate),
                        new XElement("Дата_окончания", jobEnd),
                        new XElement("Отдел", departmentName)
                    )
                ),
                new XElement("Список_зарплат",
                    new XElement("Зарплата",
                        new XElement("Год", salaryYear),
                        new XElement("Месяц", salaryMonth),
                        new XElement("Итого", salaryMonth * elapsedMonth)
                    )
                )
            );
                xdoc.Element("Сотрудники")!.Add(newEmployee);
            xdoc.Save(XmlFilePath);
            Console.WriteLine("Сотрудник добавлен успешно.");
            ShowMenu("confirmClose");
        }
    }

    static void SearchEmployeesBySurname()
    {
        XDocument xdoc = XDocument.Load(XmlFilePath);

        Console.Write("Enter employee surname to search: ");
        string surname = Console.ReadLine();

        var matchingEmployees = xdoc.Descendants("Employee")
            .Where(e => e.Element("FullName").Value.EndsWith(" " + surname))
            .ToList();

        if (matchingEmployees.Any())
        {
            foreach (var employee in matchingEmployees)
            {
                Console.WriteLine($"Employee: {employee.Element("FullName").Value}");
                // Display other details...
            }
        }
        else
        {
            Console.WriteLine("No matching employees found.");
        }
    }

    static void DisplaySalaryPayments()
    {
        XDocument xdoc = XDocument.Load(XmlFilePath);

        Console.Write("Enter start date (yyyy-MM): ");
        string startDateString = Console.ReadLine();
        DateTime startDate = DateTime.ParseExact(startDateString, "yyyy-MM", null);

        Console.Write("Enter end date (yyyy-MM): ");
        string endDateString = Console.ReadLine();
        DateTime endDate = DateTime.ParseExact(endDateString, "yyyy-MM", null);

        var salaryPayments = xdoc.Descendants("Salary")
            .Where(s => 
            {
                DateTime paymentDate = DateTime.ParseExact(s.Element("Year").Value + "-" + s.Element("Month").Value, "yyyy-MM", null);
                return paymentDate >= startDate && paymentDate <= endDate;
            })
            .ToList();

        if (salaryPayments.Any())
        {
            foreach (var payment in salaryPayments)
            {
                Console.WriteLine($"Year: {payment.Element("Year").Value}, Month: {payment.Element("Month").Value}, Total Salary: {payment.Element("TotalSalary").Value}");
            }
        }
        else
        {
            Console.WriteLine("No salary payments found for the specified period.");
        }
    }

    static void PerformDataAnalysis()
    {
       XDocument xdoc = XDocument.Load(XmlFilePath);

    // Number of currently active employees
    int activeEmployeesCount = xdoc.Descendants("Employee")
        .Count(e => e.Element("EndDate") == null || DateTime.Parse(e.Element("EndDate").Value) > DateTime.Now);

    Console.WriteLine($"Number of currently active employees: {activeEmployeesCount}");

    // List of unique job positions in each department
    var departments = xdoc.Descendants("Department")
        .Select(d => d.Value)
        .Distinct();

    foreach (var department in departments)
    {
        var uniqueJobPositions = xdoc.Descendants("Employee")
            .Where(e => e.Element("Department").Value == department)
            .Select(e => e.Element("JobTitle").Value)
            .Distinct();

        Console.WriteLine($"Unique job positions in {department} department: {string.Join(", ", uniqueJobPositions)}");
    }

    // Departments with no more than 3 employees
    var departmentsWithFewEmployees = xdoc.Descendants("Department")
        .Where(d => xdoc.Descendants("Employee")
            .Count(e => e.Element("Department").Value == d.Value) <= 3)
        .Select(d => d.Value);

    Console.WriteLine($"Departments with no more than 3 employees: {string.Join(", ", departmentsWithFewEmployees)}");

    // Employees currently working in more than one department
    var employeesInMultipleDepartments = xdoc.Descendants("Employee")
        .GroupBy(e => e.Element("FullName").Value)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key);

    Console.WriteLine($"Employees working in more than one department: {string.Join(", ", employeesInMultipleDepartments)}");

    // Years when the highest and lowest number of employees were hired or fired
    var hiringYears = xdoc.Descendants("Employee")
        .Select(e => DateTime.Parse(e.Element("StartDate").Value).Year)
        .ToList();

    var firingYears = xdoc.Descendants("Employee")
        .Where(e => e.Element("EndDate") != null)
        .Select(e => DateTime.Parse(e.Element("EndDate").Value).Year)
        .ToList();

    int maxHiringYear = hiringYears.GroupBy(y => y).OrderByDescending(g => g.Count()).First().Key;
    int minHiringYear = hiringYears.GroupBy(y => y).OrderBy(g => g.Count()).First().Key;
    int maxFiringYear = firingYears.GroupBy(y => y).OrderByDescending(g => g.Count()).First().Key;
    int minFiringYear = firingYears.GroupBy(y => y).OrderBy(g => g.Count()).First().Key;

    Console.WriteLine($"Year with the highest number of hires: {maxHiringYear}");
    Console.WriteLine($"Year with the lowest number of hires: {minHiringYear}");
    Console.WriteLine($"Year with the highest number of fires: {maxFiringYear}");
    Console.WriteLine($"Year with the lowest number of fires: {minFiringYear}");

    // Identify employees having a jubilee (major anniversary) this year
    var jubileeEmployees = xdoc.Descendants("Employee")
        .Where(e => e.Element("StartDate") != null && DateTime.Now.Year - DateTime.Parse(e.Element("StartDate").Value).Year % 5 == 0);

    Console.WriteLine("Employees with a jubilee this year:");
    foreach (var jubileeEmployee in jubileeEmployees)
    {
        Console.WriteLine($"- {jubileeEmployee.Element("FullName").Value}");
    }
    }

    static void ExportDataToXml()
    {
        XDocument xdoc = XDocument.Load(XmlFilePath);

        // Implement logic to extract analyzed data
        // For the purpose of this example, I'm creating a simple XML structure as an example.
        XElement analyzedData = new XElement("AnalyzedData");

        // Add analyzed data to the XML structure
        analyzedData.Add(new XElement("ActiveEmployeesCount", xdoc.Descendants("Employee")
            .Count(e => e.Element("EndDate") == null || DateTime.Parse(e.Element("EndDate").Value) > DateTime.Now)));

        var departments = xdoc.Descendants("Department")
            .Select(d => d.Value)
            .Distinct();

        foreach (var department in departments)
        {
            var uniqueJobPositions = xdoc.Descendants("Employee")
                .Where(e => e.Element("Department").Value == department)
                .Select(e => e.Element("JobTitle").Value)
                .Distinct();

            analyzedData.Add(new XElement("UniqueJobPositions", new XAttribute("Department", department),
                uniqueJobPositions.Select(j => new XElement("JobPosition", j))));
        }

        var departmentsWithFewEmployees = xdoc.Descendants("Department")
            .Where(d => xdoc.Descendants("Employee")
                .Count(e => e.Element("Department").Value == d.Value) <= 3)
            .Select(d => d.Value);

        analyzedData.Add(new XElement("DepartmentsWithFewEmployees",
            departmentsWithFewEmployees.Select(d => new XElement("Department", d))));

        var employeesInMultipleDepartments = xdoc.Descendants("Employee")
            .GroupBy(e => e.Element("FullName").Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        analyzedData.Add(new XElement("EmployeesInMultipleDepartments",
            employeesInMultipleDepartments.Select(e => new XElement("Employee", e))));

        // Add other analyzed data as needed...

        // Save the analyzed data to a new XML file
        analyzedData.Save("analyzed_data.xml");
        Console.WriteLine("Data exported to analyzed_data.xml");
    }

    static void FetchAndDisplayCurrencyRates()
    {
        // Implement logic to fetch and display currency rates
    }
}
