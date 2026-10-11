namespace SalonFryzjerski.Models;

public enum AppModule
{
    Home = 1,
    Uslugi = 2,
    Fryzjerzy = 3,
    Wizyty = 4,
    Grafik = 5,
    Admin = 99
}

public enum AccessLevel
{
    None = 0,
    View = 1,
    Edit = 2
}
