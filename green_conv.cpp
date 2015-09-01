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
//#pragma simd
			for (long i = 0; i < n; i++)
				result[i] += m1[i] * m2[i];
		}

		DllExport void MultiplyElementwiseAndSubtractFromResult(const long n, complex<double>* m1, complex<double>* m2, complex<double>* result)
		{
//#pragma simd
			for (long i = 0; i < n; i++)
				result[i] -= m1[i] * m2[i];
		}


		DllExport void SetAllValuesTo(const long n, MKL_Complex16* m, MKL_Complex16 value)
		{
#pragma simd
			for (long i = 0; i < n; i++)
				m[i] = value;
		}

		DllExport void AddToAll(const long n, complex<double>* m, complex<double> value)
		{
#pragma simd
			for (long i = 0; i < n; i++)
				m[i] += value;
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
	}
}