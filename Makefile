#Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov

DST=bin

FWD_PATH=./EndUser/ExtremeMt/bin/x64/Release

ifndef PLATFORM
PLATFORM=GNU
endif

user: 
	mkdir -p ${DST}
	$(MAKE) -C Native PLATFORM=$(PLATFORM)	 
	xbuild ExtrEMeMT.sln  /p:Platform="x64" /p:Configuration="Release"


	cp $(FWD_PATH)/*.exe $(DST)
	cp $(FWD_PATH)/*.dll $(DST)
	cp $(FWD_PATH)/*.so $(DST)


clean: 
	$(MAKE) -C Native clean 
	xbuild ExtrEMeMT.sln   /t:Clean /p:Platform="x64" /p:Configuration="Release"

	rm -f $(FWD_PATH)/*.dll*
	rm -f $(FWD_PATH)/*.exe*

