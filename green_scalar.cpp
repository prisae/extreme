#include <complex>
#include <vector>
//#include <mkl_service.h>
//#include <mkl_vml.h>
#include <mkl.h>

#include <iostream>

#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

using namespace std;

typedef struct{ double re; double im; } complex16;


namespace Native
{
	extern "C"
	{
		struct NativeEnvelop
		{
			int n;
			int r;
			int s;
			int t;
			int b;

			double dt;
			double db1;

			double r1;
			double r2;
			double s1;
			double s2;
		};

		DllExport void CalcEta(int length, double* lambdas, complex<double>* eta, complex<double> value)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				eta[i] = sqrt(lambdas[i] * lambdas[i] - value);
		}

		DllExport void CalcMultDivPlus(int length, complex<double>* a1, complex<double>* a2, complex<double>* a3, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] += (a1[i] * a2[i]) / a3[i];
		}

		DllExport void CalcMultDivMinus(int length, complex<double>* a1, complex<double>* a2, complex<double>* a3, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] -= (a1[i] * a2[i]) / a3[i];
		}

		DllExport void CalcDivMultPlus(int length, complex<double>* a1, complex<double>* a2, complex<double>* a3, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] += a1[i] / (a2[i] * a3[i]);
		}

		DllExport void CalcDivMultMinus(int length, complex<double>* a1, complex<double>* a2, complex<double>* a3, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] -= a1[i] / (a2[i] * a3[i]);
		}

		DllExport void CalcDivPlus(int length, complex<double>* a1, complex<double>* a2, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] += a1[i] / a2[i];
		}

		DllExport void CalcDivMinus(int length, complex<double>* a1, complex<double>* a2, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] -= a1[i] / a2[i];
		}

		DllExport void CalcExp(int length, complex<double>* eta, double factor, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] = exp(eta[i] * factor);
		}

		DllExport void CalcDoubleExp(int length, complex<double>* eta1, double factor1, complex<double>* eta2, double factor2, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] = exp(eta1[i] * factor1 + eta2[i] * factor2);
		}


		DllExport complex16 CalcDotProduct(double* hank, complex16* fk, int length, int shift)
		{
			complex16 s = { 0, 0 };

#pragma simd
			for (int j = 0; j < length; j++)
			{
				s.re += fk[shift + j].re * hank[j];
				s.im += fk[shift + j].im * hank[j];
			}

			return s;
		}
	}
}