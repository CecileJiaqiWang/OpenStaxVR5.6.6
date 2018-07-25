/*
Book Class
*/

using System;
using System.IO;
using UnityEngine;

public class Book : MonoBehaviour
{

    public GameObject Parent; // Parent of pages
    public GameObject BookInterface; // Book interface

    private string _bookChoices; // Allows you to choose which book
    private int _currentPage = 0;
    private int Progress;
    private AssetBundle _previousBundle;
    private AssetBundle _currentBundle;
    private AssetBundle _nextBundle;
    [NonSerialized]
    public Boolean IsTable = false;

    /* Returns the current number of pages loaded in the scene */
    private int TotalPageCount()
    {
        int totalPage = 4;
        if (_bookChoices == "soci")
        {
            totalPage = 25;
        }

        return totalPage;
    }

    /* Loads the current set of 10 pages from the current asset bundle*/
    private AssetBundle LoadBundle(int pageNumber)
    {

        string bundleCreation = _bookChoices + "-" + pageNumber.ToString(); // Creates the bundle name
        AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleCreation)); // Gets the bundle
      
        return bundle;
    }

    public void UnloadBundles(Book book)
    {
        
        book._currentBundle.Unload(false);
        book._previousBundle.Unload(false);
        book._nextBundle.Unload(false);
        
    }

    private void InstantiateObject(int sibIndex, AssetBundle bundle, int pageNumber)
    {
        string str = pageNumber.ToString();
        var page = Instantiate(bundle.LoadAsset<GameObject>(str), BookInterface.transform.position, BookInterface.transform.rotation); // Places the book in the scene
        page.transform.parent = Parent.transform;
        page.transform.Rotate(0, 180, 0);

        if (sibIndex != -1)
        {
            page.transform.SetSiblingIndex(sibIndex);
        }
        

    }

    /* Updates the current page displayed*/
    public void UpdatePages()
    {
        // Makes any page other than current invisible and makes current visible
        foreach (Transform child in transform)
        {
            string name = child.name;
            string num = name.Split('(')[0];
            int index = 0;
            Int32.TryParse(num, out index);
            if (index == _currentPage)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private int NextPage(int currentPage) {
        if (currentPage == TotalPageCount() - 1) {
            return 0;
        } else {
            currentPage++;
            return currentPage;
        }
    }

    private int PreviousPage(int currentPage) {
        if (currentPage == 0) {
            return TotalPageCount() - 1;
        } else {
            currentPage--;
            return currentPage;
        }
    }

    /* Goes to the next page */
    public void TurnNextPage()
    {
        _currentPage = NextPage(_currentPage);
        SaveProgress();

        GameObject toDestroy = transform.GetChild(0).gameObject;
        Destroy(toDestroy);
        _previousBundle.Unload(false);

        _previousBundle = _currentBundle;
        _currentBundle = _nextBundle;
        _nextBundle = LoadBundle(NextPage(_currentPage));
        InstantiateObject(-1, _nextBundle, NextPage(_currentPage));
        UpdatePages();
    }

    /* Goes back a page */
    public void TurnBackPage()
    {
        _currentPage = PreviousPage(_currentPage);
        SaveProgress();

        GameObject toDestroy = transform.GetChild(2).gameObject;
        Destroy(toDestroy);
        _nextBundle.Unload(false);

        _nextBundle = _currentBundle;
        _currentBundle = _previousBundle;
        _previousBundle = LoadBundle(PreviousPage(_currentPage));
        InstantiateObject(0, _previousBundle, PreviousPage(_currentPage));
        UpdatePages();
    }

    public void GoToPage(int pageNumber)
    {
        
        _currentPage = pageNumber;
        SaveProgress();

        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        _previousBundle.Unload(false);
        _currentBundle.Unload(false);
        _nextBundle.Unload(false);

        _previousBundle = LoadBundle(PreviousPage(_currentPage));
        InstantiateObject(0, _previousBundle, PreviousPage(_currentPage));
        _currentBundle = LoadBundle(_currentPage);
        InstantiateObject(1, _currentBundle, _currentPage);
        _nextBundle = LoadBundle(NextPage(_currentPage));
        InstantiateObject(2, _nextBundle, NextPage(_currentPage));
        UpdatePages();
    }

    /**
     * Save progress.
     */
    public void SaveProgress()
    {
        Progress = _currentPage;
        string currentDir = Application.persistentDataPath;        
        string fileName = _bookChoices + "envStatus.txt";
        string fullPath = currentDir + "/" + fileName;
        string bookStatus = "Progress:" + Progress;
        try
        {
            File.WriteAllText(fullPath, bookStatus);
 
        }
        catch (Exception e)
        {
            // Extract some information from this exception, and then   
            // throw it to the parent method.  
            if (e.Source != null)  
                Console.WriteLine("Exception source: {0}", e.Source);  
            throw;  
        }
    }
    
    /*
     * Load progress.
     */
    private void LoadProgress()
    {
        // The path of the txt file storing game status.
        string currentDir = Application.persistentDataPath;
        string fileName = _bookChoices + "envStatus.txt";
        string fullPath = currentDir + "/" + fileName;
        try
        {
            int bookStatus;
            // Try reading the progress.
            if (!Int32.TryParse(File.ReadAllText(fullPath).Split(':')[1], out bookStatus))
            {
                // Reset everything.
                Progress = 0;
                _currentPage = 0;
            }
            else
            {
                // Reload game.
                Progress = bookStatus;
                _currentPage = Progress;
            }
        }
        catch (Exception e)
        {
            // Create an empty file.
            File.Create(fullPath).Dispose();
            Progress = 0;
            _currentPage = 0;
        }
    }


    /* Checks for saved progress and displays pages */
    void Start()
    {
        String bookNameDir = Application.persistentDataPath + "/" + "bookName.txt";
        _bookChoices = File.ReadAllText(bookNameDir);
        LoadProgress();
        _previousBundle = LoadBundle(PreviousPage(_currentPage));
        InstantiateObject(0, _previousBundle, PreviousPage(_currentPage));
        _currentBundle = LoadBundle(_currentPage);
        InstantiateObject(1, _currentBundle, _currentPage);
        _nextBundle = LoadBundle(NextPage(_currentPage));
        InstantiateObject(2, _nextBundle, NextPage(_currentPage));
        UpdatePages();
    }

}