using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class Gamemanager : MonoBehaviour
{
    public GameObject gameCanvas;
    public GameObject endGameCanvas;
    public Text questionText;
    public Button[] answerButtons;
    public Button nextQuestionButton;
    public Text timerText;
    public Text scoreText;
    public Image questionImage;

    public AudioSource correctAnswerSound;
    public AudioSource wrongAnswerSound;

    public Text scoreDisplayText;

    public GameObject restartButton; 

    private string[] questions = {
        "Welches Sprichwort verwendet eine Metapher?",
        "Welche der folgenden Zahlen sind Primzahlen? (2)",
        "Welche Kunstepoche folgte auf die Renaissance?",
        "Welche Planeten in unserem Sonnensystem haben keine Monde? (2)",
        "Welches chemische Element hat das Symbol Fe?",
        "Wer war der erste Präsident der Vereinigten Staaten?",
        "Welche Einheit wird für die elektrische Spannung verwendet?",
        "Welche der folgenden Länder gehören nicht zur EU? (2)",
        "Wie lange brauchen Sonnenstrahlen um die Erde zu erreichen",
        "Was ist die größte Stadt der Welt nach Einwohnerzahl?"
    };

    private string[][] answers = {
        new string[] { "Der Apfel fällt nicht weit vom Stamm.", "Der frühe Vogel fängt den Wurm", "Die Zeit verfliegt wie im Flug."},
        new string[] {"19", "41", "39"},
        new string[] {"Barock", "Romantik", "Gothik"},
        new string[] {"Merkur", "Venus", "Mars"},
        new string[] {"Eisen", "Silber", "Fermium"},
        new string[] { "George Washington", "Abraham Lincoln", "Franklin D. Roosevelt"},
        new string[] {"Volt", "Watt", "Ampere"},
        new string[] {"Kroatien", "Norwegen", "Bulgarien"},
        new string[] {"~ 8 Minuten", "~ 7 Minuten", "~ 6 Minuten"},
        new string[] {"Tokyo", "Shanghai", "Dehli"},
    };

    private List<int>[] correctAnswers = {
        new List<int> {2},
        new List<int> {0, 1},
        new List<int> {0},
        new List<int> {0, 1},
        new List<int> {0},
        new List<int> {0},
        new List<int> {0},
        new List<int> {0, 1},
        new List<int> {0},
        new List<int> {0}
    };

    private int currentQuestionIndex = 0;
    private float timer = 15f;
    private bool timerRunning = false;
    private int correctAnswersCount = 0;

    public Sprite[] questionImages;
    private Dictionary<string, Sprite> questionImageMapping = new Dictionary<string, Sprite>();

    private List<int> selectedAnswers = new List<int>();
    private bool evaluationDone = false;

    void Start()
    {
        ShuffleQuestions();
        InitializeQuestionImageMapping();
        DisplayQuestion();
        nextQuestionButton.onClick.AddListener(NextQuestion);

        restartButton.GetComponent<Button>().onClick.AddListener(RestartGame);
        restartButton.SetActive(false); 
    }

    void Update()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime;
            timerText.text = "Zeit: " + Mathf.Ceil(timer).ToString();

            if (timer <= 0f)
            {
                timerRunning = false;
                if (!evaluationDone)
                    CheckAnswer(-1);
            }
        }

        nextQuestionButton.interactable = !timerRunning || (IsAnyAnswerSelected() && CanProceed());
    }

    void DisplayQuestion()
    {
        timer = 15f;
        timerRunning = true;
        evaluationDone = false; 
        timerText.text = Mathf.Ceil(timer).ToString();

        questionText.text = questions[currentQuestionIndex];

        if (questionImageMapping.ContainsKey(questions[currentQuestionIndex]))
        {
            questionImage.sprite = questionImageMapping[questions[currentQuestionIndex]];
        }

        ShuffleAnswers();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int buttonIndex = i;

            ColorBlock colors = answerButtons[i].colors;
            colors.normalColor = Color.white;
            answerButtons[i].colors = colors;

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => ToggleAnswer(buttonIndex));

            answerButtons[i].GetComponentInChildren<Text>().text = answers[currentQuestionIndex][i];

            Image buttonImage = answerButtons[i].GetComponent<Image>();
            buttonImage.color = UnityEngine.ColorUtility.TryParseHtmlString("#6B6C76", out Color color) ? color : Color.white;
        }
    }

    void ToggleAnswer(int selectedAnswer)
    {
        if (evaluationDone || selectedAnswers.Contains(selectedAnswer))
            return;

     
        selectedAnswers.Add(selectedAnswer);

        if (correctAnswers[currentQuestionIndex].Contains(selectedAnswer))
        {
            if (selectedAnswers.Count == correctAnswers[currentQuestionIndex].Count)
            {
                CheckAnswer(-1);
            }
            else
            {
                SetButtonImageColor(answerButtons[selectedAnswer], UnityEngine.ColorUtility.TryParseHtmlString("#505975", out Color pressedColor) ? pressedColor : Color.blue);
            }
        }
        else
        {

            CheckAnswer(-1);
        }

        nextQuestionButton.interactable = CanProceed();
    }

    void DeselectButton(int deselectedAnswer)
    {
        SetButtonImageColor(answerButtons[deselectedAnswer], Color.white);
    }

    bool CanProceed()
    {
        if (selectedAnswers.Count == correctAnswers[currentQuestionIndex].Count)
        {
            foreach (int selected in selectedAnswers)
            {
                if (!correctAnswers[currentQuestionIndex].Contains(selected))
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    void CheckAnswer(int dummy)
    {
        timerRunning = false;
        evaluationDone = true; 

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;

            if (correctAnswers[currentQuestionIndex].Contains(i))
            {
                SetButtonImageColor(answerButtons[i], UnityEngine.ColorUtility.TryParseHtmlString("#5781ff", out Color correctColor) ? correctColor : Color.blue);
            }
            else
            {
                SetButtonImageColor(answerButtons[i], UnityEngine.ColorUtility.TryParseHtmlString("#ff0000", out Color wrongColor) ? wrongColor : Color.red);
            }
        }

        if (IsAnswerCorrect(selectedAnswers, correctAnswers[currentQuestionIndex]))
        {
            correctAnswerSound.Play();
            correctAnswersCount++;
        }
        else
        {
            wrongAnswerSound.Play();
        }

        UpdateScoreText();

        if (scoreDisplayText != null)
        {
            scoreDisplayText.text = "YOUR SCORE: " + correctAnswersCount + " / " + questions.Length;
        }

        restartButton.SetActive(true); 
    }

    bool IsAnswerCorrect(List<int> selected, List<int> correct)
    {
        
        selected.Sort();
        correct.Sort();

        return selected.SequenceEqual(correct);
    }

    void SetButtonImageColor(Button button, Color color)
    {
        Image buttonImage = button.GetComponent<Image>();
        buttonImage.color = color;
    }

    void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Length)
        {
            selectedAnswers.Clear();
            evaluationDone = false; 

            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].interactable = true;
                SetButtonImageColor(answerButtons[i], Color.white);
            }

            DisplayQuestion();
        }
        else
        {
            gameCanvas.SetActive(false);
            endGameCanvas.SetActive(true);
            Debug.Log("Alle Fragen beantwortet!");
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + correctAnswersCount + " | " + "10";
    }

    bool IsAnyAnswerSelected()
    {
        return selectedAnswers.Count > 0;
    }

    void ShuffleQuestions()
    {
        List<int> indexes = Enumerable.Range(0, questions.Length).ToList();

        for (int i = indexes.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = indexes[i];
            indexes[i] = indexes[randomIndex];
            indexes[randomIndex] = temp;
        }

        List<string> shuffledQuestions = new List<string>();
        List<string[]> shuffledAnswers = new List<string[]>();
        List<List<int>> shuffledCorrectAnswers = new List<List<int>>();
        List<Sprite> shuffledQuestionImages = new List<Sprite>();

        foreach (int index in indexes)
        {
            shuffledQuestions.Add(questions[index]);
            shuffledAnswers.Add(answers[index]);
            shuffledCorrectAnswers.Add(correctAnswers[index]);
            shuffledQuestionImages.Add(questionImages[index]);
        }

        questions = shuffledQuestions.ToArray();
        answers = shuffledAnswers.ToArray();
        correctAnswers = shuffledCorrectAnswers.ToArray();
        questionImages = shuffledQuestionImages.ToArray();
    }

    void InitializeQuestionImageMapping()
    {
        for (int i = 0; i < questions.Length && i < questionImages.Length; i++)
        {
            questionImageMapping.Add(questions[i], questionImages[i]);
        }
    }

    void ShuffleAnswers()
    {
        List<int> indexes = Enumerable.Range(0, answers[currentQuestionIndex].Length).ToList();

        for (int i = indexes.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = indexes[i];
            indexes[i] = indexes[randomIndex];
            indexes[randomIndex] = temp;
        }

        List<string> shuffledAnswers = new List<string>();
        List<int> shuffledCorrectAnswers = new List<int>();

        foreach (int index in indexes)
        {
            shuffledAnswers.Add(answers[currentQuestionIndex][index]);

            if (correctAnswers[currentQuestionIndex].Contains(index))
            {
                shuffledCorrectAnswers.Add(shuffledAnswers.Count - 1);
            }
        }

        answers[currentQuestionIndex] = shuffledAnswers.ToArray();
        correctAnswers[currentQuestionIndex] = shuffledCorrectAnswers;
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
