#include <complex>
#include <vector>
//#include <mkl_service.h>
//#include <mkl_vml.h>
#include <mkl.h>
#include <string.h>

#include <iostream>

#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

using namespace std;

typedef struct { double re; double im; } complex16;


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
			for (int i = 0; i < length; i++)
				eta[i] = sqrt(lambdas[i] * lambdas[i] - value);
		}


		DllExport void CalcExp(int length, complex<double>* eta, double factor, complex<double>* result)
		{
#pragma simd
			for (int i = 0; i < length; i++)
				result[i] = exp(eta[i] * factor);
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

		DllExport void ClearBuffer(complex16* buffer, size_t length)
		{
			memset(buffer, 0, length * 16);
		}

		void my_zaxpy(int n, double alpha, complex16* x, complex16* y)
		{
			complex16 cAlpha = { alpha, 0 };
			cblas_zaxpy(n, &cAlpha, x, 1, y, 1);
		}

		void my_zcopy(int n, complex16* x, complex16* y)
		{
			cblas_zcopy(n, x, 1, y, 1);
		}


		void my_vzexp(int n, complex16* buffer)
		{
			complex<double>* c = (complex<double>*)buffer;

#pragma simd
			for (size_t i = 0; i < n; i++)
				c[i] = exp(c[i]);

			//vmzExp(n, (MKL_Complex16*)buffer, (MKL_Complex16*)buffer, VML_HA);
		}


		DllExport void CalculateT(NativeEnvelop* ne, complex16* eta, complex16* result)
		{
			int n = ne->n;
			complex16 zero = { 0, 0 };


			complex16* etaR = eta + n * ne->r;
			complex16* etaS = eta + n * ne->s;
			complex16* etaT = eta + n * ne->t;

			complex16* buffer = new complex16[n];
			ClearBuffer(result, n);

			//exp(-eta[r] * z.r1 - eta[s] * z.s1 + 2 * eta[t] * dt) -
			//exp(-eta[r] * z.r1 - eta[s] * z.s2 + 2 * eta[t] * dt) -
			//exp(-eta[r] * z.r2 - eta[s] * z.s1 + 2 * eta[t] * dt) +
			//exp(-eta[r] * z.r2 - eta[s] * z.s2 + 2 * eta[t] * dt);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->r1, etaR, buffer);
			my_zaxpy(n, -ne->s1, etaS, buffer);
			my_zaxpy(n, 2 * ne->dt, etaT, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);


			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->r1, etaR, buffer);
			my_zaxpy(n, -ne->s2, etaS, buffer);
			my_zaxpy(n, 2 * ne->dt, etaT, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->r2, etaR, buffer);
			my_zaxpy(n, -ne->s1, etaS, buffer);
			my_zaxpy(n, 2 * ne->dt, etaT, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);


			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->r2, etaR, buffer);
			my_zaxpy(n, -ne->s2, etaS, buffer);
			my_zaxpy(n, 2 * ne->dt, etaT, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			delete[] buffer;
		}

		DllExport void CalculateR(NativeEnvelop* ne, complex16* eta, complex16* result)
		{
			int n = ne->n;
			complex16 zero = { 0, 0 };

			complex16* etaB = eta + n * ne->b;
			complex16* etaS = eta + n * ne->s;
			complex16* etaR = eta + n * ne->r;

			complex16* buffer = new complex16[n];

			ClearBuffer(result, n);


			/*exp(-2 * eta[b] * db1 + eta[s] * z.s2 + eta[r] * z.r2) -
			exp(-2 * eta[b] * db1 + eta[s] * z.s1 + eta[r] * z.r2) -
			exp(-2 * eta[b] * db1 + eta[s] * z.s2 + eta[r] * z.r1) +
			exp(-2 * eta[b] * db1 + eta[s] * z.s1 + eta[r] * z.r1);*/

			ClearBuffer(buffer, n);
			my_zaxpy(n, -2 * ne->db1, etaB, buffer);
			my_zaxpy(n, ne->s2, etaS, buffer);
			my_zaxpy(n, ne->r2, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -2 * ne->db1, etaB, buffer);
			my_zaxpy(n, ne->s1, etaS, buffer);
			my_zaxpy(n, ne->r2, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -2 * ne->db1, etaB, buffer);
			my_zaxpy(n, ne->s2, etaS, buffer);
			my_zaxpy(n, ne->r1, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -2 * ne->db1, etaB, buffer);
			my_zaxpy(n, ne->s1, etaS, buffer);
			my_zaxpy(n, ne->r1, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			delete[] buffer;
		}

		DllExport void CalculateCTop(NativeEnvelop* ne, complex16* eta, complex16* result)
		{
			int n = ne->n;
			complex16 zero = { 0, 0 };

			complex16* etaS = eta + n * ne->s;
			complex16* etaR = eta + n * ne->r;

			complex16* buffer = new complex16[n];

			ClearBuffer(result, n);

			/*exp(-eta[s] * z.s1 + eta[r] * z.r2) -
			exp(-eta[s] * z.s2 + eta[r] * z.r2) -
			exp(-eta[s] * z.s1 + eta[r] * z.r1) +
			exp(-eta[s] * z.s2 + eta[r] * z.r1); */

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->s1, etaS, buffer);
			my_zaxpy(n, ne->r2, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->s2, etaS, buffer);
			my_zaxpy(n, ne->r2, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->s1, etaS, buffer);
			my_zaxpy(n, ne->r1, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->s2, etaS, buffer);
			my_zaxpy(n, ne->r1, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			delete[] buffer;
		}



		DllExport void CalculateCBot(NativeEnvelop* ne, complex16* eta, complex16* result)
		{
			int n = ne->n;
			complex16 zero = { 0, 0 };

			complex16* etaS = eta + n * ne->s;
			complex16* etaR = eta + n * ne->r;

			complex16* buffer = new complex16[n];

			ClearBuffer(result, n);


			//exp(-eta[r] * z.r1 + eta[s] * z.s2) -
			//exp(-eta[r] * z.r1 + eta[s] * z.s1) -
			//exp(-eta[r] * z.r2 + eta[s] * z.s2) +
			//exp(-eta[r] * z.r2 + eta[s] * z.s1);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->r1, etaR, buffer);
			my_zaxpy(n, ne->s2, etaS, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->r1, etaR, buffer);
			my_zaxpy(n, ne->s1, etaS, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->r2, etaR, buffer);
			my_zaxpy(n, ne->s2, etaS, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			ClearBuffer(buffer, n);
			my_zaxpy(n, -ne->r2, etaR, buffer);
			my_zaxpy(n, ne->s1, etaS, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			delete[] buffer;
		}


		DllExport void CalculateDTop(NativeEnvelop* ne, complex16* eta, complex16* result)
		{
			int n = ne->n;
			complex16 zero = { 0, 0 };

			complex16* etaS = eta + n * ne->s;
			complex16* etaR = eta + n * ne->r;
			complex16* etaB = eta + n * ne->b;
			complex16* etaT = eta + n * ne->t;


			complex16* buffer = new complex16[n];
			complex16* firstAddendum = new complex16[n];

			ClearBuffer(result, n);

			/*	var firstAddendum = -2 * (eta[b] * db1 - eta[t] * dt);

			exp(firstAddendum + eta[s] * z.s2 - eta[r] * z.r1) -
			exp(firstAddendum + eta[s] * z.s1 - eta[r] * z.r1) -
			exp(firstAddendum + eta[s] * z.s2 - eta[r] * z.r2) +
			exp(firstAddendum + eta[s] * z.s1 - eta[r] * z.r2);
			*/

			ClearBuffer(firstAddendum, n);
			my_zaxpy(n, -2 * ne->db1, etaB, firstAddendum);
			my_zaxpy(n, 2 * ne->dt, etaT, firstAddendum);

			my_zcopy(n, firstAddendum, buffer);
			my_zaxpy(n, ne->s2, etaS, buffer);
			my_zaxpy(n, -ne->r1, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			my_zcopy(n, firstAddendum, buffer);
			my_zaxpy(n, ne->s1, etaS, buffer);
			my_zaxpy(n, -ne->r1, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			my_zcopy(n, firstAddendum, buffer);
			my_zaxpy(n, ne->s2, etaS, buffer);
			my_zaxpy(n, -ne->r2, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			my_zcopy(n, firstAddendum, buffer);
			my_zaxpy(n, ne->s1, etaS, buffer);
			my_zaxpy(n, -ne->r2, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			delete[] firstAddendum;
			delete[] buffer;
		}

		DllExport void CalculateDBot(NativeEnvelop* ne, complex16* eta, complex16* result)
		{
			int n = ne->n;
			complex16 zero = { 0, 0 };

			complex16* etaS = eta + n * ne->s;
			complex16* etaR = eta + n * ne->r;
			complex16* etaB = eta + n * ne->b;
			complex16* etaT = eta + n * ne->t;


			complex16* buffer = new complex16[n];
			complex16* firstAddendum = new complex16[n];

			ClearBuffer(result, n);

			//var firstAddendum = -2 * (eta[b] * db1 - eta[t] * dt);

			//exp(firstAddendum - eta[s] * z.s1 + eta[r] * z.r2) -
			//exp(firstAddendum - eta[s] * z.s2 + eta[r] * z.r2) -
			//exp(firstAddendum - eta[s] * z.s1 + eta[r] * z.r1) +
			//exp(firstAddendum - eta[s] * z.s2 + eta[r] * z.r1);

			ClearBuffer(firstAddendum, n);
			my_zaxpy(n, -2 * ne->db1, etaB, firstAddendum);
			my_zaxpy(n, 2 * ne->dt, etaT, firstAddendum);

			my_zcopy(n, firstAddendum, buffer);
			my_zaxpy(n, -ne->s1, etaS, buffer);
			my_zaxpy(n, ne->r2, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			my_zcopy(n, firstAddendum, buffer);
			my_zaxpy(n, -ne->s2, etaS, buffer);
			my_zaxpy(n, ne->r2, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			my_zcopy(n, firstAddendum, buffer);
			my_zaxpy(n, -ne->s1, etaS, buffer);
			my_zaxpy(n, ne->r1, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, -1, buffer, result);

			my_zcopy(n, firstAddendum, buffer);
			my_zaxpy(n, -ne->s2, etaS, buffer);
			my_zaxpy(n, ne->r1, etaR, buffer);
			my_vzexp(n, buffer);
			my_zaxpy(n, 1, buffer, result);

			delete[] firstAddendum;
			delete[] buffer;
		}

	}
}