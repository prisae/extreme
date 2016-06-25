//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
#include "algebra.h"

#ifdef WINDOWS
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT 
#endif

DLLEXPORT void Zaxpy(const long n, complex16 alpha, complex16 *x, long incX, complex16 *y, long incY)
{
	complex16 alpha1 = alpha;
	cblas_zaxpy((blasint)n, (complex_ptr)&alpha1, (complex_ptr)x, (blasint)incX, (complex_ptr)y, (blasint)incY);
}

DLLEXPORT void Zcopy(const long n, complex16*x, complex16 *y)
{
	cblas_zcopy((blasint)n, (complex_ptr)x, one_int, (complex_ptr)y, one_int);
}

DLLEXPORT void Zrot(const int n, complex16*x, int incX, complex16*y, int incY, double c, complex16 s)
{
	blasint n1 = n;
	blasint incX1 = incX;
	blasint incY1 = incY;
	zrot(&n1, (complex_ptr)x, &incX1, (complex_ptr)y, &incY1, &c, (complex_ptr)&s);
}

DLLEXPORT void Zrotg(complex16 a, complex16 b, double* c, complex16* s)
{
	zrotg((complex_ptr)&a, (complex_ptr)&b, c, (complex_ptr)s);
}

DLLEXPORT void SimplifiedZtrsv(const int n, complex16 *a, int lda, complex16 *x)
{
	cblas_ztrsv(CblasColMajor, CblasUpper, CblasNoTrans, CblasNonUnit, (blasint)n, (complex_ptr)a, lda, (complex_ptr)x, one_int);
}

DLLEXPORT void ZgemvNotTrans(int n, int jh, complex16 alpha, complex16* a, complex16* input, complex16 beta, complex16* result)
{
	cblas_zgemv(CblasColMajor, CblasNoTrans, (blasint)n, (blasint)jh, (complex_ptr)&alpha,
		(complex_ptr)a, (blasint)n, (complex_ptr)input, one_int, (complex_ptr)&beta, (complex_ptr)result, one_int);
}


DLLEXPORT void ZgemvConjTrans(int m, int n, complex16 alpha, complex16* a, complex16* input, complex16 beta, complex16* result)
{
	cblas_zgemv(CblasRowMajor, CblasConjTrans, (blasint)m, (blasint)n, (complex_ptr)&alpha, (complex_ptr)a, (blasint)n, (complex_ptr)input, one_int, (complex_ptr)&beta, (complex_ptr)result, one_int);
}


DLLEXPORT double Dznrm2(int n, complex16 *x)
{
	return cblas_dznrm2((blasint)n, (complex_ptr)x, one_int);
}
