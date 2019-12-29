/*
using System;
using System.Runtime.CompilerServices;

public class Matrix<T> {
	public T this[int i,int j] {
		get {

			return data[i,j];
		}
	}
	public int n {
		get {return _n;}
	}
	public int m {
		get {return _m;}
	}

	protected T[,] data;
	protected int _n;
	protected int _m;

	public Matrix(int n, int m) {
		this._n = n;
		this._m = m;
	}

	public Matrix(T[,] data) {
		this._n = data.GetLength(0);
		this._m = data.GetLength(1);
	}

	[DynamicAttributes(a)]
	public static Matrix<T> operator* (dynamic a, Matrix<T> m) {
		var data = new T[m.n, m.m];
		for (int i = 0; i < m.n; i++) {
			for (int j = 0; j < m.m; j++) {
				data[i,j] = a * m[i,j];
			}
		}
		return new Matrix<T>(data);
	}
}

public struct RationalNumber {
	public bool negative {
		get {return this._negative;}
		set {this._negative = value;}
	}
	public uint nominator {
		get {return this._nominator;}
		set {
			this._nominator = value;
			ConvertToBasicForm();
		}
	}
	public uint denominator {
		get {return this._denominator;}
		set {
			if (value == 0) throw new DivideByZeroException();
			this._denominator = value;
			ConvertToBasicForm();
		}
	}
	private bool _negative;
	private uint _nominator;
	private uint _denominator;

	public RationalNumber(int nom, int denom) {
		if (denom == 0) throw new DivideByZeroException();
		this._negative = (nom > 0 != denom > 0) && nom != 0;
		this._nominator = (uint) nom;
		this._denominator = (uint) denom;
		ConvertToBasicForm();
	}

	public RationalNumber(uint nom, uint denom, bool negative = false) {
		if (denom == 0) throw new DivideByZeroException();
		this._negative = negative;
		this._nominator = nom;
		this._denominator = denom;
		ConvertToBasicForm();
	}

	public RationalNumber(ulong nom, ulong denom, bool negative = false) {
		if (denom == 0) throw new DivideByZeroException();
		this._negative = negative;
		//External conversion to Basic Form because of the long datatypes
		if (nom == 0) {
			this._negative = false;
			denom = 1;
		} else {
			ulong div = GCD(nom, denom);
			nom /= div;
			denom /= div;
		}
		this._nominator = (uint) nom;
		this._denominator = (uint) denom;
	}

	public void ConvertToBasicForm() {
		if (this._nominator == 0) {
			this._negative = false;
			this._denominator = 1;
			return;
		}
		uint gcd = GCD(this._nominator, this._denominator);
		this._nominator /= gcd;
		this._denominator /= gcd;
	}

	public static RationalNumber operator+ (RationalNumber a, RationalNumber b) {
		long x = ((long) a._nominator * b._denominator * ((a._negative)?-1:1) + (long) b._nominator * a._denominator * ((b._negative)?-1:1));
		return new RationalNumber((ulong) x, ((ulong) a._denominator) * b._denominator, x < 0);
	}

	public static RationalNumber operator+ (RationalNumber a, int b) {
		long x = ((long) a._nominator * ((a._negative)?-1:1) + (long) b * a._denominator);
		return new RationalNumber((ulong) x, (ulong) a._denominator, x < 0);
	}

	public static RationalNumber operator+ (int b, RationalNumber a) {
		long x = ((long) a._nominator * ((a._negative)?-1:1) + (long) b * a._denominator);
		return new RationalNumber((ulong) x, (ulong) a._denominator, x < 0);
	}

	public static RationalNumber operator- (RationalNumber a, RationalNumber b) {
		long x = ((long) a._nominator * b._denominator * ((a._negative)?-1:1) - (long) b._nominator * a._denominator * ((b._negative)?-1:1));
		return new RationalNumber((ulong) x, (ulong) a._denominator * b._denominator, x < 0);
	}

	public static RationalNumber operator- (RationalNumber a, int b) {
		long x = ((long) a._nominator * ((a._negative)?-1:1) - (long) b * a._denominator);
		return new RationalNumber((ulong) x, (ulong) a._denominator, x < 0);
	}

	public static RationalNumber operator- (int b, RationalNumber a) {
		long x = ((long) a._nominator * ((a._negative)?1:-1) + (long) b * a._denominator);
		return new RationalNumber((ulong) x, (ulong) a._denominator, x < 0);
	}

	public static RationalNumber operator* (RationalNumber a, RationalNumber b) {
		return new RationalNumber((ulong) a._nominator * b._nominator, (ulong) a._denominator * b._denominator, a._negative != b._negative);
	}

	public static RationalNumber operator* (RationalNumber a, int b) {
		return new RationalNumber((ulong) a._nominator * (uint) b, (ulong) a._denominator, b < 0 != a._negative);
	}

	public static RationalNumber operator* (int b, RationalNumber a) {
		return new RationalNumber((ulong) a._nominator * (uint) b, (ulong) a._denominator, b < 0 != a._negative);
	}

	public static RationalNumber operator/ (RationalNumber a, RationalNumber b) {
		return new RationalNumber((ulong) a._nominator * b._nominator, (ulong) a._denominator * b._denominator, a._negative != b._negative);
	}

	public static RationalNumber operator/ (RationalNumber a, int b) {
		return new RationalNumber((ulong) a._nominator, (ulong) a._denominator * (uint) b, b < 0 != a._negative);
	}

	public static RationalNumber operator/ (int b, RationalNumber a) {
		return new RationalNumber((ulong) a._denominator * (uint) b, (ulong) a._nominator, b < 0 != a._negative);
	}

	public static bool operator== (RationalNumber a, RationalNumber b) {
		return (a._denominator == b._denominator) && (a._nominator == b._nominator) && (a._negative == b._negative);
	}

	public static bool operator== (RationalNumber a, int b) {
		return (a._denominator == 1) && (a._nominator == (uint) b) && (a._negative == b < 0);
	}

	public static bool operator== (int b, RationalNumber a) {
		return (a._denominator == 1) && (a._nominator == (uint) b) && (a._negative == b < 0);
	}

	public static bool operator!= (RationalNumber a, RationalNumber b) {
		return (a._denominator != b._denominator) || (a._nominator != b._nominator) || (a._negative != b._negative);
	}

	public static bool operator!= (RationalNumber a, int b) {
		return (a._denominator != 1) || (a._nominator != (uint) b) || (a._negative != b < 0);
	}

	public static bool operator!= (int b, RationalNumber a) {
		return (a._denominator != 1) || (a._nominator != (uint) b) || (a._negative != b < 0);
	}

	public static implicit operator int (RationalNumber r) {
		if (r._denominator == 1) {
			return (int) r._nominator * (r._negative?-1:1);
		}
	    return (int) r._nominator * (r._negative?-1:1) / (int) r._denominator;
	}

	public static implicit operator float (RationalNumber r) {
	    return (float) r._nominator * (r._negative?-1:1) / (float) r._denominator;
	}

	public static uint GCD(uint a, uint b) {
		return b == 0 ? a : GCD(b, a % b);
	}

	public static ulong GCD(ulong a, ulong b) {
		return b == 0 ? a : GCD(b, a % b);
	}

	public override bool Equals(object obj) {
		if (obj is RationalNumber) {
			return this == (RationalNumber) obj;
		}
		if (obj is int) {
			return this == (int) obj;
		}
		if (obj is uint) {
			return this == (uint) obj;
		}
		if (obj is float) {
			return (float) this == (float) obj;
		}
		return false;
	}

	public override int GetHashCode() {
		return ((this._negative?"-":"")+this._nominator+","+this._denominator).GetHashCode();
	}
}
*/