#include <iostream>

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
		DllExport void Transpose(size_t rows, size_t cols, complex16* data)
		{
			complex16 one = { 1, 0 };
#ifdef _MKL_H_	
                        mkl_zimatcopy('R', 'T', rows, cols, one, data, cols, rows);

#else
			cblas_zimatcopy(CblasRowMajor, CblasTrans, (blasint)rows, (blasint)cols, (complex_ptr)&one,(complex_ptr) data,(blasint) cols,(blasint) rows);
#endif
		}

		DllExport void TransposeBlock(size_t rows, size_t cols, complex16* data, size_t lda, size_t ldb)
		{
			complex16 one = { 1, 0 };

#ifdef _MKL_H_	
                        mkl_zimatcopy('R', 'T', rows, cols, one, data, lda, ldb);


#else
			cblas_zimatcopy(CblasRowMajor, CblasTrans, (blasint)rows,(blasint) cols,(complex_ptr) &one, (complex_ptr) data, (blasint)lda,(blasint) ldb);
#endif
		}
	}
}
