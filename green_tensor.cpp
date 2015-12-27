//#include <mkl_service.h>
//#include <mkl_vml.h>
//#include <mkl.h>

#include "algebra.h"
#include <iostream>

#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

using namespace std;

//typedef struct { double re; double im; } complex16;

namespace Native
{
	extern "C"
	{
		DllExport void Zaxpy(const long n, complex16 alpha, complex16 *x, int incX, complex16 *y, int incY)
		{
			cblas_zaxpy((blasint)n,(complex_ptr) &alpha,(complex_ptr) x, (blasint)incX,(complex_ptr) y, (blasint)incY);
		}

		DllExport void Zcopy(const long n,complex16*x, complex16 *y)
		{
			cblas_zcopy((blasint)n, (complex_ptr)x, one_int,(complex_ptr) y, one_int);
		}

		DllExport void Zrot(const int n, complex16*x, blasint incX, complex16*y, blasint incY, double c, complex16 s)
		{
			zrot((blasint*)&n,(complex_ptr)x, (blasint*)&incX, (complex_ptr)y,(blasint*) &incY, &c,(complex_ptr) &s);
		}

		DllExport void Zrotg(complex16 a,complex16 b, double* c, complex16* s)
		{
			zrotg((complex_ptr)&a,(complex_ptr) &b,  c,(complex_ptr) s);
		}

		DllExport void SimplifiedZtrsv(const int n, complex16 *a, blasint lda, complex16 *x)
		{
			cblas_ztrsv(CblasColMajor, CblasUpper, CblasNoTrans, CblasNonUnit, (blasint)n, (complex_ptr) a, lda, (complex_ptr)x,one_int);
		}

		DllExport void ZgemvNotTrans(int n, int jh, complex16 alpha, complex16* a, complex16* input, complex16 beta, complex16* result)
		{
			cblas_zgemv(CblasColMajor, CblasNoTrans, (blasint)n, (blasint)jh,(complex_ptr) &alpha,
					(complex_ptr) a,(blasint) n,(complex_ptr) input, one_int,(complex_ptr) &beta,(complex_ptr) result, one_int);
		}
	

		DllExport void ZgemvConjTrans(blasint m, blasint n, complex16 alpha, complex16* a, complex16* input, complex16 beta, complex16* result)
		{
			cblas_zgemv(CblasRowMajor, CblasConjTrans, m, n, (complex_ptr)&alpha,(complex_ptr) a, n, (complex_ptr)input, one_int,(complex_ptr) &beta,(complex_ptr) result, one_int);
		}


		DllExport double Dznrm2(blasint n, complex16 *x)
		{
			return cblas_dznrm2(n,(complex_ptr) x,one_int );

		}
	}
}
