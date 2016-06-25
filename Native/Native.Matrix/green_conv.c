//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
#include "algebra.h"

#ifdef WINDOWS
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT 
#endif

DLLEXPORT void Zscal(const long n, complex16 alpha, complex16* result)
{
	cblas_zscal((blasint)n, (complex_ptr)&alpha, (complex_ptr)result, one_int);
}

DLLEXPORT void MultiplyElementwiseAndAddToResult(const long n, complex16* m1, complex16* m2, complex16* result)
{
	//#pragma simd
	for (long i = 0; i < n; i++)
	{
		complex16 x = m1[i];
		complex16 y = m2[i];

		result[i].re += x.re*y.re - x.im*y.im;
		result[i].im += x.im*y.re + x.re*y.im;
	}
}

DLLEXPORT void MultiplyElementwiseAndSubtractFromResult(const long n, complex16* m1, complex16* m2, complex16* result)
{
	//#pragma simd
	for (long i = 0; i < n; i++)
	{
		complex16 x = m1[i];
		complex16 y = m2[i];

		result[i].re -= x.re*y.re - x.im*y.im;
		result[i].im -= x.im*y.re + x.re*y.im;
	}
}

DLLEXPORT void SetAllValuesTo(const long n, complex16* m, complex16 value)
{
#pragma simd
	for (long i = 0; i < n; i++)
		m[i] = value;
}

DLLEXPORT void AddToAll(const long n, complex16* m, complex16 value)
{
#pragma simd
	for (long i = 0; i < n; i++)
	{
		m[i].re += value.re;
		m[i].im += value.im;
	}
}

DLLEXPORT void Copy(const long n, complex16* src, complex16* dst)
{
	cblas_zcopy((blasint)n, (complex_ptr)src, one_int, (complex_ptr)dst, one_int);
}

DLLEXPORT void AddElementwise(const long n, complex16* m1, complex16* m2, complex16* result)
{
	for (long i = 0; i < n; i++)
	{
		result[i].re = m1[i].re + m2[i].re;
		result[i].im = m1[i].im + m2[i].im;
	}
}

DLLEXPORT void SubtractElementwise(const long n, complex16* m1, complex16* m2, complex16* result)
{
	for (long i = 0; i < n; i++)
	{
		result[i].re = m1[i].re - m2[i].re;
		result[i].im = m1[i].im - m2[i].im;
	}
}

DLLEXPORT void MultiplyElementwise(const long n, complex16* m1, complex16* m2, complex16* result)
{
	for (long i = 0; i < n; i++)
	{
		complex16 x = m1[i];
		complex16 y = m2[i];

		result[i].re = x.re*y.re - x.im*y.im;
		result[i].im = x.im*y.re + x.re*y.im;
	}
}

DLLEXPORT void CalculateAlphaX(const long n, complex16 alpha, complex16* result)
{
	cblas_zscal((blasint)n, (complex_ptr)&alpha, (complex_ptr)result, one_int);
}

DLLEXPORT void CalculateDotProductConjugatedLeft(const long n, complex16* m1, complex16* m2, complex16* result)
{
	cblas_zdotc_sub((blasint)n, (complex_ptr)m1, one_int, (complex_ptr)m2, one_int, (complex_ptr2)result);
}

DLLEXPORT void CalculateDotProductNotConjugated(const long n, complex16* m1, complex16* m2, complex16* result)
{
	cblas_zdotu_sub((blasint)n, (complex_ptr)m1, one_int, (complex_ptr)m2, one_int, (complex_ptr2)result);
}
