#include <mkl_service.h>
#include <mkl_vml.h>
#include <mkl.h>

#include <iostream>
#include <complex>

#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

using namespace std;


namespace Native
{
	extern "C"
	{

		DllExport void Zscal(const long n, complex<double> alpha, complex<double>* result)
		{
			cblas_zscal(n, &alpha, result, 1);
		}

		DllExport void MultiplyElementwiseAndAddToResult(const long n, complex<double>* m1, complex<double>* m2, complex<double>* result)
		{
#pragma simd
			for (long i = 0; i < n; i++)
				result[i] += m1[i] * m2[i];

			//(r1*r2 - i1*i2) + i*(r1*i2 + i1*r2).

			//vzMul(n, m1, m2, result);

			//y := alpha*A*x + beta*y,
			// order, TransA, M, N, *alpha, *A, lda, *X, incX, *beta, *Y,  incY);

			/*	MKL_Complex16 one;
			one.real = 1;
			one.imag = 0;


			cblas_zgemv(CblasRowMajor, CblasNoTrans, 1, n, &one, m1, n, m2, 1, &one, result, 1);

			cblas_zdotc_sub(n, m1, 1, m2, 1, result);*/
		}




		DllExport void SetAllValuesTo(const long n, MKL_Complex16* m, MKL_Complex16 value)
		{
#pragma simd
			for (long i = 0; i < n; i++)
				m[i] = value;
		}

		DllExport void AddToAll(const long n, MKL_Complex16* m, MKL_Complex16 value)
		{
			for (long i = 0; i < n; i++)
			{
				m[i].real = m[i].real + value.real;
				m[i].imag = m[i].imag + value.imag;
			}
		}

		DllExport void Copy(const long n, MKL_Complex16* src, MKL_Complex16* dst)
		{
			cblas_zcopy(n, src, 1, dst, 1);
		}

		DllExport void AddElementwise(const long n, MKL_Complex16* m1, MKL_Complex16* m2, MKL_Complex16* result)
		{
			vzAdd(n, m1, m2, result);
		}

		DllExport void SubtractElementwise(const long n, MKL_Complex16* m1, MKL_Complex16* m2, MKL_Complex16* result)
		{
			vzSub(n, m1, m2, result);
		}

		DllExport void MultiplyElementwise(const long n, MKL_Complex16* m1, MKL_Complex16* m2, MKL_Complex16* result)
		{
			vzMul(n, m1, m2, result);
		}

		DllExport void CalculateAlphaX(const long n, MKL_Complex16 alpha, MKL_Complex16* result)
		{
			cblas_zscal(n, &alpha, result, 1);
		}

		DllExport void CalculateDotProductConjugatedLeft(const long n, MKL_Complex16* m1, MKL_Complex16* m2, MKL_Complex16* result)
		{
			cblas_zdotc_sub(n, m1, 1, m2, 1, result);
		}

		DllExport void CalculateDotProductNotConjugated(const long n, MKL_Complex16* m1, MKL_Complex16* m2, MKL_Complex16* result)
		{
			cblas_zdotu_sub(n, m1, 1, m2, 1, result);
		}

		DllExport void MultiplyElementwiseAndSubtractFromResult(const long n, MKL_Complex16* m1, MKL_Complex16* m2, MKL_Complex16* result)
		{
			for (long i = 0; i < n; i++)
			{
				result[i].real = result[i].real - m1[i].real * m2[i].real + m1[i].imag*m2[i].imag;
				result[i].imag = result[i].imag - m1[i].real * m2[i].imag - m1[i].imag*m2[i].real;
			}

			//(r1*r2 - i1*i2) + i*(r1*i2 + i1*r2).

			//vzMul(n, m1, m2, result);

			//y := alpha*A*x + beta*y,
			// order, TransA, M, N, *alpha, *A, lda, *X, incX, *beta, *Y,  incY);

			/*	MKL_Complex16 one;
			one.real = 1;
			one.imag = 0;


			cblas_zgemv(CblasRowMajor, CblasNoTrans, 1, n, &one, m1, n, m2, 1, &one, result, 1);

			cblas_zdotc_sub(n, m1, 1, m2, 1, result);*/
		}
	}
}