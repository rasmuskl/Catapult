require 'albacore'

def get_build_number
	yearversion = Time.now.strftime("%Y")
	dateversion = Time.now.strftime("%m%d")
	buildnumber = Time.now.strftime("%H%M")
	"#{$major_version}.#{yearversion}.#{dateversion}.#{buildnumber}"
end

$msbuildpath = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe'
$major_version = '1'
$build_number = get_build_number

$publishdir = 'Publish'
$builddir = 'Source/AlphaLaunch.App/bin/Release/app.publish'
$includePattern = '/**/*'

task :default => [:full]

task :full => [:clean, :compile, :publish, :copyfiles] do
	puts $build_number
end

desc "Removes old build artifacts."
msbuild :clean do |msb|
	rm_rf 'Artifacts'
	rm_rf 'Publish'
	msb.command = $msbuildpath
	msb.properties :configuration => :Release
	msb.targets :Clean
	msb.solution = "Source/AlphaLaunch.App.sln"	
end

msbuild :fetchslndeps do |msb|
	msb.command = $msbuildpath
	msb.targets :RestorePackages
	msb.solution = "Source/.nuget/NuGet.targets"	
end	

msbuild :compile => [:fetchslndeps] do |msb|
	msb.command = $msbuildpath
	msb.properties :configuration => :Release, :ApplicationVersion => $build_number
	msb.targets :Build
	msb.solution = "Source/AlphaLaunch.App.sln"	
end

msbuild :publish => [:compile] do |msb|
	msb.command = $msbuildpath
	msb.properties :configuration => :Release, :ApplicationVersion => $build_number, :PublishUrl => 'http://alphalaunch.rasmuskl.dk/setup/'
	msb.targets :Publish
	msb.solution = "Source/AlphaLaunch.App.sln"	
end

task :copyfiles => [:publish] do
	rm_rf 'Publish'
	mkdir 'Publish'
	FileList[$builddir + $includePattern].each do |f|
		targetLocation = f.sub($builddir, $publishdir)
		mkdir_p File.dirname(targetLocation)
		if not File.directory?(f)
			cp f, targetLocation
		end
	end
end

task :uploadfiles do
	FileList[$publishdir + $includePattern].each do |f|
		targetLocation = f.sub($publishdir, 'setup')
		system 'tools/curl/curl.exe "ftp://alphalaunch.rasmuskl.dk/Alphalaunch/'+targetLocation+'" --ssl --insecure --ftp-create-dirs --user ' + ENV['ALPHALAUNCH_FTP'] + ' -T "'+f+'"'
	end
end