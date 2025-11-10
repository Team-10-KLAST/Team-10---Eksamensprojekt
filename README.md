# Team-10---Eksamensprojekt

## Team kode kvalitetskriterier

//TODO

### Mappestruktur 
- Presentation.Wpf/                        (*UI-lag (WPF, MVVM, ressourcer)*)
     - Views/
     - ViewModels/
     - Commands/ fx RelayCommand
     -  Helpers/                               (*fx Converters*)
     -  Resources/                             (*Styles, farver, templates*)
     -  Assets/Images/                         (*Ikoner, billeder til UI*)

- Application/                               (*Forretningslogik, kontrakter og domænemodeller*)
   - Interfaces/
        - Service                            (*Service kontrakter*)
        - Repository                         (*Repository kontrakter*)
   - Services/                               (*Service implementationer*)
   - Models/


- Data/                                   (*Infrastruktur og dataadgang*)
  - AdoNet/                               (*Repositories fx DeviceRepository*)
  - SqlConnectionFactory.cs               
