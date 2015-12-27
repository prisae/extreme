#include <complex>
//#include <iostream>
#include "algebra.h"


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
		DllExport void ZgemvAsymTrans(int nz, complex16* alpha, complex16* beta, complex16* green, complex16* input, complex16* result)
		{
			cblas_zgemv(CblasRowMajor, CblasTrans, nz, nz, (complex_ptr) alpha, (complex_ptr)green,
				       	nz, (complex_ptr)input, one_int, (complex_ptr) beta, (complex_ptr)result, one_int);
		}

		DllExport void ZgemvAsymNoTrans(int nz, complex16* alpha, complex16* beta, complex16* green, complex16* input, complex16* result)
		{
			cblas_zgemv(CblasRowMajor, CblasNoTrans, nz, nz, (complex_ptr)alpha,(complex_ptr) green,
				       	nz, (complex_ptr)input, one_int,(complex_ptr) beta,(complex_ptr) result, one_int);
		}

		DllExport void ZgemvAtoO(int n, int m, complex16* alpha, complex16* beta, complex16* green, complex16* input, complex16* result)
		{
			cblas_zgemv(CblasRowMajor, CblasTrans, n, m, (complex_ptr)alpha, (complex_ptr)green,
				       	m, (complex_ptr)input, one_int, (complex_ptr)beta, (complex_ptr)result, one_int);
		}

		DllExport void ZgemvSym(int nz, complex16* alpha, complex16* beta, complex16* green, complex16* input, complex16* result)
		{
			//const char u = 'U';
			char u = 'U';
			blasint one = 1;
			blasint n=nz;
			zspmv(&u, &n, (complex_ptr)alpha, (complex_ptr)green, (complex_ptr)input,
				       	&one, (complex_ptr)beta, (complex_ptr)result, &one);
		}

		DllExport void FullZgemv(int nz, blasint length,
			complex16* xx,
			complex16* xy,
			complex16* xz,
			complex16* yy,
			complex16* yz,
			complex16* zz,
			complex16* src,
			complex16* dst)
		{
			blasint asymNz = nz * nz;
			blasint symmNz = nz + nz * (nz - 1) / 2;

			complex16 one = { 1, 0 };
			complex16 minusOne = { -1, 0 };
			complex16 zero = { 0, 0 };

			blasint dataShift;
			blasint symmShift;
			blasint asymShift;

			complex16* dstX;
			complex16* dstY;
			complex16* dstZ;

			complex16* srcX;
			complex16* srcY;
			complex16* srcZ;

			//#pragma omp parallel  for private (dataShift, symmShift, asymShift, dstX, dstY, dstZ, srcX, srcY, srcZ)
			//#pragma parallel always
			for (int i = 0; i < length; i++)
			{
				dataShift = i * 3 * nz;
				symmShift = i * symmNz;
				asymShift = i * asymNz;

				dstX = dst + dataShift;
				dstY = dst + dataShift + nz;
				dstZ = dst + dataShift + nz + nz;

				srcX = src + dataShift;
				srcY = src + dataShift + nz;
				srcZ = src + dataShift + nz + nz;

				ZgemvSym(nz, &one, &zero, xx + symmShift, srcX, dstX);
				ZgemvSym(nz, &one, &one, xy + symmShift, srcY, dstX);
				ZgemvAsymNoTrans(nz, &one, &one, xz + asymShift, srcZ, dstX);

				ZgemvSym(nz, &one, &zero, xy + symmShift, srcX, dstY);
				ZgemvSym(nz, &one, &one, yy + symmShift, srcY, dstY);
				ZgemvAsymNoTrans(nz, &one, &one, yz + asymShift, srcZ, dstY);

				ZgemvAsymTrans(nz, &minusOne, &zero, xz + asymShift, srcX, dstZ);
				ZgemvAsymTrans(nz, &minusOne, &one, yz + asymShift, srcY, dstZ);
				ZgemvSym(nz, &one, &one, zz + symmShift, srcZ, dstZ);
			}
		}

	}
	
}
