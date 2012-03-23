root:=$(CURDIR)
tr_name := Translator.exe
tr_cmd_out := -out:$(tr_name)
thisdir := .
cmd_compiler := dmcs
ig_sources := ItaniumGenerator.cs ItaniumImmediate.cs ItaniumInstruction.cs ItaniumObject.cs ItaniumRegister.cs RuleGenerator.cs
tr_sources := Translator.cs RuleGenerator.cs
lib_dir := -lib:Libraries/Collections \
			  -lib:Libraries/LexicalAnalysis \
			  -lib:Libraries/Parsing \
			  -lib:Libraries/Starlight \
			  -lib:Libraries/Extensions \
			  -lib:Libraries/Tycho
options := -r:Libraries.Collections.dll \
			  -r:Libraries.LexicalAnalysis.dll \
			  -r:Libraries.Parsing.dll \
			  -r:Libraries.Extensions.dll \
			  -r:Libraries.Tycho.dll \
			  -r:Libraries.Starlight.dll
debug_options := -r:Libraries.Collections.dll \
			  -r:Libraries.LexicalAnalysis.dll \
			  -r:Libraries.Parsing.dll \
			  -r:Libraries.Extensions.dll \
			  -r:Libraries.Tycho.dll \
			  -r:Libraries.Starlight.dll \
			  --debug
Translator: parsing copy
	$(cmd_compiler) $(tr_cmd_out) $(lib_dir) $(options) $(tr_sources)

Translator-Debug: parsing copy
	$(cmd_compiler) $(tr_cmd_out) $(lib_dir) $(tr_debug_options) $(tr_sources)

copy: 
	cd $(root)/Libraries/LexicalAnalysis/ && cp *.dll $(root)/; \
	cd $(root)/Libraries/Extensions/ && cp *.dll $(root)/; \
	cd $(root)/Libraries/Collections/ && cp *.dll $(root)/; \
	cd $(root)/Libraries/Starlight/ && cp *.dll $(root)/; \
	cd $(root)/Libraries/Tycho/ && cp *.dll $(root)/; \
	cd $(root)/Libraries/Parsing/ && cp *.dll $(root)/; 

clean: 
	rm *.exe *.dll \
	cd $(root)/Libraries/LexicalAnalysis && $(MAKE) clean; \
	cd $(root)/Libraries/Extensions && $(MAKE) clean; \
	cd $(root)/Libraries/Collections && $(MAKE) clean; \
	cd $(root)/Libraries/Tycho && $(MAKE) clean; \
	cd $(root)/Libraries/Starlight && $(MAKE) clean; \
	cd $(root)/Libraries/Parsing && $(MAKE) clean; \

collections: extensions 
	cd $(root)/Libraries/Collections && $(MAKE) 

starlight: 
	cd $(root)/Libraries/Starlight && $(MAKE) 

tycho: lexical 
	cd $(root)/Libraries/Tycho && $(MAKE) 

parsing: tycho starlight  
	cd $(root)/Libraries/Parsing && $(MAKE)

extensions: 
	cd $(root)/Libraries/Extensions && $(MAKE)

lexical: collections 
	cd $(root)/Libraries/LexicalAnalysis && $(MAKE)
		
