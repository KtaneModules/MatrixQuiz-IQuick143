using UnityEngine;
using UnityEngine.UI;

public class MatrixDisplay : MonoBehaviour {
	[SerializeField]
	private GameObject MatrixEntryPrefab;
	[SerializeField]
	private Transform MatrixHolder;
	private Text[,] matrixNumbers;
	[SerializeField]
	private Text QuestionText;

	void Awake() {
		this.matrixNumbers = new Text[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				var GO = GameObject.Instantiate(MatrixEntryPrefab, this.MatrixHolder);
				GO.name = i+" "+j;
				this.matrixNumbers[i,j] = GO.GetComponent<Text>();
			}
		}
	}

	public void DrawMatrix(int[,] data) {
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				this.matrixNumbers[i,j].text = data[i,j].ToString();
			}
		}
	}

	public void ShowQuestion(string question) {
		QuestionText.text = question;
	}
}
