namespace PhotoTimeline
open System
open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.MetaData.Profiles.Exif
open System.Globalization

module TimelineCreator =

    type RenameFileStatus =
        | Pending 
        | Success 
        | Failed

    type RenameFile =
        {
            Filename: string
            NewLocation: string
            Status: RenameFileStatus
            DateTaken: DateTime
        }

    let log (m: string) = 
       System.Console.WriteLine m

    let getDateTaken (file: string) =
        let image = Image.Load(string file)
        let exif = image.MetaData.ExifProfile
        let value = exif.GetValue(ExifTag.DateTimeOriginal).ToString()
        let d = DateTime.ParseExact(value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture).AddDays(1.0)
        file, d    

    let sameFileDateAndSize source destination = 
        let _, destinationDate = getDateTaken destination
        let datesEqual = (DateTime.Compare(source.DateTaken, destinationDate) = 0)

        let sourceFileInfo = new FileInfo(source.Filename)
        let destinationFileInfo = new FileInfo(destination);
        let sizeEqual = (sourceFileInfo.Length = destinationFileInfo.Length);
        datesEqual && sizeEqual

    let createNewName file = 
        let newName = Path.GetDirectoryName(file.NewLocation) + @"\" + Path.GetFileNameWithoutExtension(file.NewLocation) + "_1" + Path.GetExtension(file.NewLocation); //FileInfo?
        { Filename = file.Filename; NewLocation = newName; Status = file.Status; DateTaken = file.DateTaken }

    let copyFile s d = 
        let path = Path.GetDirectoryName (string d)
        if not (Directory.Exists path) then
            Directory.CreateDirectory(path) |> ignore
        else
            () 
        File.Copy(s, d)   

    let rec handleDuplicate f = 
        log ("Handeling" + f.NewLocation)
        if not (File.Exists f.NewLocation) then
            log ("doesn't exist, copy")
            copyFile f.Filename f.NewLocation
            ()
        elif sameFileDateAndSize f f.NewLocation then
            log ("Same file with same date & size")
            ()
        else
            log ("Creating new name for :" + f.NewLocation)
            let newName = createNewName(f)
            log (newName.NewLocation)
            newName |> handleDuplicate

    let relocateFiles files =
        Seq.iter (handleDuplicate) files

    let readFolders folders = 
        Array.collect (fun f -> Directory.GetFiles(f)) folders

    let getWeekOfYear date =
        CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)

    let createNewLocation d (f: string, datetaken: DateTime) =
        let destinationDirectory = d + String.Format("\\{0}\\Week {1}", datetaken.Year, getWeekOfYear(datetaken)); 
        let destinationFilename = datetaken.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(f)    
        let destinationFile = Path.Combine(destinationDirectory, destinationFilename);
        { Filename = f; NewLocation = destinationFile; Status = RenameFileStatus.Pending; DateTaken = datetaken }

    let determineNewLocation destinationFolder files = 
        Seq.map (getDateTaken >> createNewLocation destinationFolder) files

    let processFolder sourceFolders destinationFolder = 
        sourceFolders
            |> readFolders
            |> determineNewLocation destinationFolder
            |> relocateFiles
            |> ignore
        0


