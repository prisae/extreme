#include <mkl_service.h>
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
		DllExport void ZgemvAsymTrans(int nz, complex16* alpha, complex16* beta, void* green, void* input, void* result)
		{
			cblas_zgemv(CblasColMajor, CblasTrans, nz, nz, alpha, green, nz, input, 1, beta, result, 1);
		}

		DllExport void ZgemvAsymNoTrans(int nz, complex16* alpha, complex16* beta, void* green, void* input, void* result)
		{
			cblas_zgemv(CblasColMajor, CblasNoTrans, nz, nz, alpha, green, nz, input, 1, beta, result, 1);
		}

		DllExport void ZgemvSym(int nz, MKL_Complex16* alpha, MKL_Complex16* beta, MKL_Complex16* green, MKL_Complex16* input, MKL_Complex16* result)
		{
			const char u = 'U';
			int one = 1;

			zspmv(&u, &nz, alpha, green, input, &one, beta, result, &one);
		}
	}
}