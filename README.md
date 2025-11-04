# Team-10---Eksamensprojekt

## Team kode kvalitetskriterier

//TODO

### Mappestruktur 
- Presentation.Wpf/                         UI-lag (WPF, MVVM, ressourcer) 
     - Views/
     - ViewModels/
     - Commands/                               fx RelayCommand
     -  Helpers/                               fx Converters
     -  Resources/                             Styles, farver, templates
     -  Assets/Images/                         Ikoner, billeder til UI 

- Application/                                Use cases /services
   - Interfaces/                              Service kontrakter
   -  Services/                               Service implementationer
 
- Domain/                                     Forretningslogik og domænemodeller 
     - Models/
     - Interfaces/                           Interfaces til dataadgang

- Data/                                    Infrastruktur og dataadgang
  - AdoNet/                                Repositories fx DeviceRepository
  - EfCore/                              (Hvis vi skifter )
  - SqlConnectionFactory.cs               
