using System.Collections.Generic;
using UnityEngine;

public class LinearAlgebraModule : MonoBehaviour {
	[SerializeField]
	private KMBombModule module;
	[SerializeField]
	private MatrixDisplay display;
	[SerializeField]
	private KMSelectable YesButton;
	[SerializeField]
	private KMSelectable NoButton;
	[SerializeField]
	private new KMAudio audio;

	private int[][,] matrices;
	private string[] questions;
	private int[] answers;
	private int questionIndex = 0;
	private int numQuestions = 0;

	private bool moduleSolved = false;
	
	// Things used for logging
	private static int moduleIdCounter = 1;
	private int moduleId;

	void Awake() {
		moduleId = moduleIdCounter;
		moduleIdCounter++;

		YesButton.OnInteract += delegate() {return HandlePress(true);};
		NoButton.OnInteract += delegate() {return HandlePress(false);};
	}

	void Start() {
		Log("Generating Questions");
		float difficulty = 15;
		List<QuestionType> questionTypes = new List<QuestionType>();
		while (difficulty > 1) {
			QuestionType q = RandomQuestionType();
			float diff = GetDifficulty(q);
			//This if statement makes sure that we don't try t o fit in a very difficult question even tho the test is already very close to the target diff
			if (Mathf.Abs(difficulty - diff) < difficulty) {
				questionTypes.Add(q);
				difficulty -= diff;
			}
		}
		this.numQuestions = questionTypes.Count;
		this.matrices = new int[this.numQuestions][,];
		this.questions = new string[this.numQuestions];
		this.answers = new int[this.numQuestions];
		int iter = 0;
		foreach (var q in questionTypes) {
			int i = 0;
			float best = float.MaxValue;
			while (best > 300 && i < 20) {
				i++;
				var gen = GenerateQuestion(q);
				var mat = (int[,]) gen[0];
				var quest = (string) gen[1];
				var answ = (int) gen[2];
				var extremes = MatrixExtremes(mat);
				int score = Mathf.Max(extremes[1], -extremes[0]);
				if (score < best || i == 20) {
					best = score;
					this.matrices[iter] = mat;
					this.questions[iter] = quest;
					this.answers[iter] = answ;
				}
			}
			Log(
				"Generated question "+(iter+1)+
				": Matrix:"+MatrixToString(this.matrices[iter])+
				" Answer:"+((this.answers[iter]==-1)?"Any (Error)":((this.answers[iter]==0)?"No":"Yes"))+
				" Question:"+this.questions[iter]);
			iter++;
		}
		ShowQuestion();
	}

	private bool HandlePress(bool answer) {
		var button = answer?YesButton:NoButton;
		Log("\""+(answer?"Yes":"No")+"\" button pressed.");
		button.AddInteractionPunch();
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
		HandleAnswer(answer);
		return false;
	}

	private void HandleAnswer(bool answer) {
		if (moduleSolved) return;
		int correctAnswer = this.answers[this.questionIndex];
		bool correct = (correctAnswer == -1)?true:((correctAnswer == 1) == answer);
		if (correct) {
			Log("Correct input, advancing to the next question");
			NextQuestion();
			if (!moduleSolved) {
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, this.transform);
			}
		} else {
			Log("Incorrect input, awarding a strike. Got:"+(answer?"Yes":"No")+" Expected:"+(!answer?"Yes":"No"));
			module.HandleStrike();
		}
	}

	private void NextQuestion() {
		if (moduleSolved) return;
		this.questionIndex++;
		if (questionIndex >= this.numQuestions) {
			moduleSolved = true;
			module.HandlePass();
			display.ShowQuestion("Congratulations?");
			Log("Module was successfully solved.");
		} else ShowQuestion();
	}

	private void ShowQuestion() {
		Log("Displaying question:"+ (this.questionIndex+1) + "/" + this.numQuestions);
		display.DrawMatrix(this.matrices[this.questionIndex]);
		display.ShowQuestion(this.questions[this.questionIndex]);
	}

	private static string MatrixToString(int[,] mat) {
		string msg = "";
		for (int i = 0; i < mat.GetLength(0); i++) {
			msg += "[";
			for (int j = 0; j < mat.GetLength(1); j++) {
				msg += mat[i,j].ToString() + " ";
			}
			msg += "]";
		}
		return msg;
	}

	private static int[,] RandomCentroSymmetricMatrix(int min = -10, int max = 10) {
		int[,] mat = new int[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = i; j < 3; j++) {
				int a = Random.Range(min, max+1);
				mat[i,j] = a;
				mat[2-i,2-j] = a;
			}
		}
		return mat;
	}

	private static int[,] RandomSkewSymmetricMatrix(int min = -10, int max = 10) {
		int[,] mat = new int[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < i; j++) {
				int a = Random.Range(min, max+1);
				mat[i,j] =  a;
				mat[j,i] = -a;
			}
		}
		return mat;
	}

	private static int[,] RandomSymmetricMatrix(int min = -10, int max = 10) {
		int[,] mat = new int[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j <= i; j++) {
				int a = Random.Range(min, max+1);
				mat[i,j] = a;
				mat[j,i] = a;
			}
		}
		return mat;
	}

	private static int[,] RandomMatrix(int min = -10, int max = 10) {
		int[,] mat = new int[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				mat[i,j] = Random.Range(min, max+1);
			}
		}
		return mat;
	}

	private static int[,] RandomIdempotentMatrix(int randomscale = 7) {
		int[,] D = new int[3,3];
		D[0,0] = Random.Range(0, 2);
		D[1,1] = Random.Range(0, 2);
		D[2,2] = Random.Range(0, 2);
		if (D[0,0] == D[1,1] && D[1,1] == D[2,2]) {
			int q = Random.Range(0, 3);
			if (D[0,0] == 0) {
				D[q, q] = 1;
			} else {
				D[q, q] = 0;
			}
		}
		int[][,] changeOfBasis = RandomIntegerInvertibleMatrix3x3(randomscale);
		return Multiply(Multiply(changeOfBasis[0], D), changeOfBasis[1]);
	}

	private static int[,] RandomInvolutoryMatrix(int randomscale = 7) {
		int[,] D = new int[3,3];
		D[0,0] = Random.Range(0, 2) == 0?1:-1;
		D[1,1] = Random.Range(0, 2) == 0?1:-1;
		D[2,2] = Random.Range(0, 2) == 0?1:-1;
		if (D[0,0] == D[1,1] && D[1,1] == D[2,2]) {
			int q = Random.Range(0, 3);
			D[q, q] *= -1;
		}
		int[][,] changeOfBasis = RandomIntegerInvertibleMatrix3x3(randomscale);
		return Multiply(Multiply(changeOfBasis[0], D), changeOfBasis[1]);
	}

	private static int[,] RandomDiagonalizableMatrix(int randomscale = 3) {
		int[,] D = new int[3,3];
		D[0,0] = Random.Range(-randomscale, randomscale+1);
		D[1,1] = Random.Range(-randomscale, randomscale+1);
		D[2,2] = Random.Range(-randomscale, randomscale+1);
		int[][,] changeOfBasis = RandomIntegerInvertibleMatrix3x3(randomscale);
		return Multiply(Multiply(changeOfBasis[0], D), changeOfBasis[1]);
	}

	private static int[,] RandomNonDiagonalizableMatrix(int randomscale = 5, int eigvalRandom = 3) {
		var changeOfBasis = RandomIntegerInvertibleMatrix3x3(randomscale);
		return Multiply(Multiply(changeOfBasis[0], RandomNonTrivialJordanForm3x3(eigvalRandom)), changeOfBasis[1]);
	}

	private static int[,] RandomNonTrivialJordanForm3x3(int randomscale = 3) {
		int[,] J = new int[3,3];
		int l_1 = Random.Range(randomscale, randomscale+1);
		int l_2 = Random.Range(randomscale, randomscale+1);
		J[0,0] = l_1;
		J[0,1] = 1;
		J[1,1] = l_1;
		J[1,2] = (l_1==l_2)?1:0;
		J[2,2] = l_2;
		return J;
	}

	//<summary>
	//	Returns an integer matrix and it's inverse such that they're both integer matrices
	//</summary>
	private static int[][,] RandomIntegerInvertibleMatrix3x3(int randomscale = 5) {
		var mat = RandomDet1Matrix3x3(0, randomscale);
		return new int[2][,]{mat, IntegerInverse3x3(mat)};
	}

	//<summary>
	//	Returns an integer matrix with determinant 1
	//  Larger iteration degeree makes larger but more random matrices (and takes a fuckton more computation)
	//</summary>
	private static int[,] RandomDet1Matrix3x3(int iterationDegree = 0, int randomscale = 5) {
		if (iterationDegree == 0) {
			return Multiply(Transpose(RandomLowerTriangularMatrix3x3(-randomscale, randomscale, true)), RandomLowerTriangularMatrix3x3(-randomscale, randomscale, true));
		} else {
			return Multiply(RandomDet1Matrix3x3(iterationDegree-1, randomscale), RandomDet1Matrix3x3(iterationDegree-1, randomscale));
		}
	}
	
	private static int[,] RandomDet0Matrix3x3(int randomscale = 5) {
		var q = RandomLowerTriangularMatrix3x3(-randomscale, randomscale, true);
		int a = Random.Range(0, 3);
		q[a,a] = 0;
		return Multiply(Transpose(q), RandomLowerTriangularMatrix3x3(-randomscale, randomscale, true));
	}

	private static int[,] RandomLowerTriangularMatrix3x3(int min = -10, int max = 10, bool onesDiagonal = false) {
		int[,] mat = new int[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < i; j++) {
				mat[i,j] = Random.Range(min, max+1);
			}
			if (onesDiagonal) {
				mat[i,i] = 1;
			} else {
				mat[i,i] = Random.Range(min, max+1);
			}
		}
		return mat;
	}

	private static int[,] Transpose(int[,] a) {
		int n = a.GetLength(0);
		int m = a.GetLength(1);
		int[,] b = new int[m, n];
		for (int i = 0; i < n; i++) {
			for (int j = 0; j < m; j++) {
				b[j,i] = a[i,j];
			}
		}
		return b;
	}

	private static int[,] Multiply(int[,] a, int[,] b) {
		int n = a.GetLength(0);
		int k = a.GetLength(1);
		//CHECK b.GetLength(0) == k
		int m = b.GetLength(1);
		int[,] x = new int[n, m];
		for (int i = 0; i < n; i++) {
			for (int j = 0; j < m; j++) {
				for (int l = 0; l < k; l++) {
					x[i,j] += a[i,l] * b[l,j];
				}
			}
		}
		return x;
	}

	private static int[] MatrixExtremes(int[,] mat) {
		int min = mat[0,0];
		int max = mat[0,0];
		for (int i = 0; i < mat.GetLength(0); i++) {
			for (int j = 0; j < mat.GetLength(1); j++) {
				if (mat[i,j] > max) {
					max = mat[i,j];
				}
				if (mat[i,j] < min) {
					min = mat[i,j];
				}
			}
		}
		return new int[]{min,max};
	}

	private static int Trace(int[,] mat) {
		//Check if a is a square matrix
		int a = 0;
		for (int i = 0; i < mat.GetLength(0); i++) {
			a += mat[i,i];
		}
		return a;
	}

	private static int Determinant3x3(int[,] a) {
		return (
			a[0,0]*(a[1,1]*a[2,2] - a[1,2]*a[2,1]) 
		  - a[0,1]*(a[1,0]*a[2,2] - a[1,2]*a[2,0])
		  + a[0,2]*(a[1,0]*a[2,1] - a[1,1]*a[2,0])
		);
	}

	private static int Permanent3x3(int[,] a) {
		return (
			a[0,0]*(a[1,1]*a[2,2] + a[1,2]*a[2,1]) 
		  + a[0,1]*(a[1,0]*a[2,2] + a[1,2]*a[2,0])
		  + a[0,2]*(a[1,0]*a[2,1] + a[1,1]*a[2,0])
		);
	}

	private static int Rank3x3(int[,] a) {
		return a.GetLength(0) - Nullity3x3(a);
	}

	private static int Nullity3x3(int[,] a) {
		var e = RowEchelonForm3x3(a);
		int nullity = 0;
		for (int i = 2; i >= 0; i--) {
			for (int j = 2; j >= 0; j--) {
				if (e[i,j] != 0) {
					goto End;
				}
			}
			nullity++;
		}
		End:
			return nullity;
	}

	//I know this is unreadable, sorry, blame past IQuick
	//In his defense he was probably tired
	private static int[,] RowEchelonForm3x3(int[,] a) {
		int r = 0;
		int k = 0;
		long[,] B = new long[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				B[i,j] = a[i,j];
			}
		}
		while (r < 3 && k < 3) {
			int index = 0;
			long i_maxVal = -1;
			for (int i = r; i < 3; i++) {
				long abs = (B[i, k] > 0)?B[i, k]:-B[i, k];
				if (abs > i_maxVal) {
					i_maxVal = abs;
					index = i;
				}
			}
			if (B[index, k] != 0) {
				if (index != r) {
					for (int i = 0; i < 3; i++) {
						long temp = B[index, i];
						B[index, i] = B[r, i];
						B[r, i] = temp;
					}
				}
				for (int i = r+1; i < 3; i++) {
					if (B[i, k] != 0) {
						for (int j = k+1; j < 3; j++) {
							B[i, j] = B[i, j] * B[r, k] - B[i, k] * B[r, j];
						}
						B[i, k] = 0;
					}
				}
				r++;
			}
			k++;
		}
		int[,] x = new int[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				x[i,j] = (int) B[i,j];
			}
		}
		return x;
	}

	private static int[,] IntegerInverse3x3(int[,] a) {
		//Check if dim = 3,3
		//Check if det = 1
		int[,] x = new int[3,3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				x[i,j] = Cofactor3x3(a, j, i);
			}
		}
		return x;
	}

	private static int Cofactor3x3(int[,] mat, int x, int y) {
		return (((x+y)%2)==0?1:-1) * Minor3x3(mat, x, y);
	}

	private static int Minor3x3(int[,] mat, int x, int y) {
		int a = x==0?1:0;
		int b = x==2?1:2;
		int c = y==0?1:0;
		int d = y==2?1:2;
		return mat[c,a] * mat[d,b] - mat[d,a] * mat[c,b];
	}

	private enum QuestionType : int {
		Triangularity = 1,
		Diagonality = 2,

		Symmetry = 3,
		SkewSymmetry = 4,
		CentroSymmetry = 5,

		Rank = 6,
		Nullity = 7,

		Determinant = 8,
		Permanent = 9,
		Trace = 10,
		Minor = 11,
		Cofactor = 12,

		Invertibility = 13,
		Involutory = 14,
		Idempotent = 15,
		Diagbility = 16
	}

	private static QuestionType RandomQuestionType() {
		return (QuestionType) Random.Range(1, 17);
	}

	private static float GetDifficulty(QuestionType t) {
		switch (t) {
			case QuestionType.Triangularity: return 0.25f;
			case QuestionType.Diagonality:   return 0.25f;
			case QuestionType.Symmetry:      return 0.25f;
			case QuestionType.SkewSymmetry:  return 0.25f;
			case QuestionType.CentroSymmetry:return 0.25f;
			
			case QuestionType.Trace:         return 1;
			case QuestionType.Determinant:   return 3.75f;
			case QuestionType.Permanent:     return 3.75f;
			case QuestionType.Minor:         return 3f;
			case QuestionType.Cofactor:      return 3.25f;

			case QuestionType.Nullity:       return 5;
			case QuestionType.Rank:          return 5;

			case QuestionType.Invertibility: return 4;
			case QuestionType.Involutory:    return 6;
			case QuestionType.Idempotent:    return 6.25f;
			case QuestionType.Diagbility:    return 10f;

			default: return 1;
		}
	}

	private static object[] GenerateQuestion(QuestionType t) {
		string question = "YOU SHOULD NOT SEE THIS TEXT";
		int[,] mat = {
			{1337,1337,1337},
			{1337,1337,1337},
			{1337,1337,1337}
		};
		int answer = -1;
		switch (t) {
			case QuestionType.Triangularity: {
				answer = Random.Range(0,2);
				question = "Is A triangular?";
				if (answer == 0) {
					mat = RandomMatrix(-99, 99);
					if (mat[0,1] == 0 && mat[0,2] == 0 && mat[1,2] == 0) answer = 1;
				} else {
					mat = RandomLowerTriangularMatrix3x3(-99, 99);
					if (Random.Range(0,2) == 0) mat = Transpose(mat);
				}
				break;
			}
			case QuestionType.Diagonality: {
				answer = Random.Range(0,2);
				question = "Is A diagonal?";
				if (answer == 0) {
					mat = RandomMatrix(-99, 99);
					if (
						mat[0,1] == 0 && mat[0,2] == 0 && mat[1,2] == 0 &&
						mat[1,0] == 0 && mat[2,0] == 0 && mat[2,1] == 0
					) answer = 1;
				} else {
					mat = new int[,]{
						{Random.Range(-99, 100),0,0},
						{0,Random.Range(-99, 100),0},
						{0,0,Random.Range(-99, 100)}
					};
				}
				break;
			}
			case QuestionType.Symmetry: {
				answer = Random.Range(0,2);
				question = "Is A symmetric?";
				if (answer == 0) {
					mat = RandomMatrix(-99, 99);
					if (
						mat[0,1] == mat[1,0] && mat[0,2] == mat[2,0] && mat[1,2] == mat[2,1]
					) answer = 1;
				} else {
					mat = RandomSymmetricMatrix(-99, 99);
				}
				break;
			}
			case QuestionType.SkewSymmetry: {
				answer = Random.Range(0,2);
				question = "Is A skew-symmetric?";
				if (answer == 0) {
					mat = RandomMatrix(-99, 99);
					if (
						mat[0,1] == -mat[1,0] && mat[0,2] == -mat[2,0] && mat[1,2] == -mat[2,1] &&
						mat[0,0] == 0 && mat[1,1] == 0 && mat[2,2] == 0
					) answer = 1;
				} else {
					mat = RandomSkewSymmetricMatrix(-99, 99);
				}
				break;
			}
			case QuestionType.CentroSymmetry: {
				answer = Random.Range(0,2);
				question = "Is A centrosymmetric?";
				if (answer == 0) {
					mat = RandomMatrix(-99, 99);
					if (
						mat[0,0] == mat[2,2] && mat[0,1] == mat[2,1] && mat[0,2] == mat[2,0] && mat[1,0] == mat[1,2]
					) answer = 1;
				} else {
					mat = RandomCentroSymmetricMatrix(-99, 99);
				}
				break;
			}
			
			case QuestionType.Trace: {
				answer = Random.Range(0,2);
				mat = RandomMatrix(-99, 99);
				int val = Trace(mat);
				if (answer == 0) {
					val += Random.Range(1, 10) * ((Random.Range(0, 2) == 0)?-1:1);
				}
				question = "Does tr A = "+val+"?";
				break;
			}
			case QuestionType.Determinant: {
				answer = Random.Range(0,2);
				mat = RandomMatrix(-20, 20);
				int val = Determinant3x3(mat);
				if (answer == 0) {
					val += Random.Range(1, 10) * ((Random.Range(0, 2) == 0)?-1:1);
				}
				question = "Does det A = "+val+"?";
				break;
			}
			case QuestionType.Permanent: {
				answer = Random.Range(0,2);
				mat = RandomMatrix(-20, 20);
				int val = Permanent3x3(mat);
				if (answer == 0) {
					val += Random.Range(1, 10) * ((Random.Range(0, 2) == 0)?-1:1);
				}
				question = "Does perm A = "+val+"?";
				break;
			}
			case QuestionType.Minor: {
				answer = Random.Range(0,2);
				mat = RandomMatrix(-20, 20);
				int x = Random.Range(0, 3);
				int y = Random.Range(0, 3);
				int val = Minor3x3(mat, x, y);
				if (answer == 0) {
					val += Random.Range(1, 10) * ((Random.Range(0, 2) == 0)?-1:1);
				}
				question = "Does M"+(x+1)+","+(y+1)+" of A = "+val+"?";
				break;
			}
			case QuestionType.Cofactor: {
				answer = Random.Range(0,2);
				mat = RandomMatrix(-20, 20);
				int x = Random.Range(0, 3);
				int y = Random.Range(0, 3);
				int val = Cofactor3x3(mat, x, y);
				if (answer == 0) {
					val += Random.Range(1, 10) * ((Random.Range(0, 2) == 0)?-1:1);
				}
				question = "Does C"+(x+1)+","+(y+1)+" of A = "+val+"?";
				break;
			}

			case QuestionType.Nullity: {
				bool rand = Random.Range(0,2) == 1;
				mat = rand?RandomDet0Matrix3x3(6):RandomMatrix(-99, 99);
				int a = Random.Range(0,3);
				answer = (a == Nullity3x3(mat))?1:0;
				question = "Does null A = "+a+"?";
				break;
			}
			case QuestionType.Rank: {
				mat = RandomMatrix(-99, 99);
				int a = Random.Range(0,3);
				answer = (a == Rank3x3(mat))?1:0;
				question = "Does rank A = "+a+"?";
				break;
			}

			case QuestionType.Invertibility: {
				answer = Random.Range(0,2);
				if (answer == 0) {
					mat = RandomDet0Matrix3x3(6);
				} else {
					mat = RandomMatrix(-99, 99);
					if (Determinant3x3(mat) == 0) answer = 0;
				}
				question = "Is A invertible?";
				break;
			}
			case QuestionType.Involutory: {
				answer = Random.Range(0,2);
				if (answer == 1) {
					mat = RandomInvolutoryMatrix(7);
				} else {
					mat = RandomMatrix(-99, 99);
					var matsqr = Multiply(mat, mat);
					if (
						matsqr[0,0] == 1 && matsqr[0,1] == 0 && matsqr[0,2] == 0 && 
						matsqr[1,0] == 0 && matsqr[1,1] == 1 && matsqr[1,2] == 0 && 
						matsqr[2,0] == 0 && matsqr[2,1] == 0 && matsqr[2,2] == 1 
					) answer = 1;
				}
				question = "Is A involutory?";
				break;
			}
			case QuestionType.Idempotent: {
				answer = Random.Range(0,2);
				if (answer == 1) {
					//Debug.LogWarning("LEEEROY");
					mat = RandomIdempotentMatrix(7);
				} else {
					mat = RandomMatrix(-99, 99);
					var matsqr = Multiply(mat, mat);
					if (
						matsqr[0,0] == mat[0,0] && matsqr[0,1] == mat[0,1] && matsqr[0,2] == mat[0,2] && 
						matsqr[1,0] == mat[1,0] && matsqr[1,1] == mat[1,1] && matsqr[1,2] == mat[1,2] && 
						matsqr[2,0] == mat[2,0] && matsqr[2,1] == mat[2,1] && matsqr[2,2] == mat[2,2] 
					) answer = 1;
				}
				question = "Is A idempotent?";
				break;
			}
			case QuestionType.Diagbility: {
				answer = Random.Range(0,2);
				if (answer == 1) {
					mat = RandomDiagonalizableMatrix(7);
				} else {
					mat = RandomNonDiagonalizableMatrix();
				}
				question = "Is A diagonalizable?";
				break;
			}
		}
		return new object[]{mat, question, answer};
	}

	private void Log(string message) {
		Debug.LogFormat("[Linear Algebra Matrix Quiz #{0}] "+message, moduleId);
	}

	[HideInInspector]
	public string TwitchHelpMessage = "Press a button to answer. Use \"press/answer yes/y/no/n\". Examples: answer yes, press y, answer n, press no";
	public KMSelectable[] ProcessTwitchCommand(string command) {
		Log("Recieved command:"+command);
		command = command.ToLowerInvariant().Trim();
		var match = System.Text.RegularExpressions.Regex.Match(command, "(?:press|answer) (y(?:es)?|no?).*");
		if (match.Success) {
			Log(match.Groups[1].ToString());
			char answer = match.Groups[1].Value[0];
			if (answer == 'n') {
				return new KMSelectable[]{this.NoButton};
			}
			if (answer == 'y') {
				return new KMSelectable[]{this.YesButton};
			}
		}
		return null;
	}
}
