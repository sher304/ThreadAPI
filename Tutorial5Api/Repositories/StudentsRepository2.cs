using Lecture5.Models;

namespace Lecture5.Repositories;

public class StudentsRepository2
{
    public async Task<IEnumerable<Student>> GetStudentsAsync()
    {
        await Task.Delay(2000); 
        
        List<Student> students = new List<Student>();
        
        students.Add(new Student{IndexNumber = "s1234", FirstName = "John", LastName = "Doe" });
        students.Add(new Student{IndexNumber = "s1235", FirstName = "Jane", LastName = "Doe" });
        students.Add(new Student{IndexNumber = "s1236", FirstName = "Jane", LastName = "Doe" });
       
        return students; 
    }
}