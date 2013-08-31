require 'albacore'

$msbuildpath = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe'
$aspnet_compiler_path = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_compiler.exe'
$publishdir = 'Publish'
$major_version = '1'

task :default => [:full]

task :full => [:clean, :compile, :publish] do
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
	msb.properties :configuration => :Release
	msb.targets :Build
	msb.solution = "Source/AlphaLaunch.App.sln"	
end

msbuild :publish => [:compile] do |msb|
	msb.command = $msbuildpath
	msb.properties :configuration => :Release, :ApplicationVersion => get_build_number
	msb.targets :Publish
	msb.solution = "Source/AlphaLaunch.App.sln"	
end

def get_build_number
	yearversion = Time.now.strftime("%Y")
	dateversion = Time.now.strftime("%m%d")
	buildnumber = ENV["BUILD_NUMBER"].nil? ? '0' : ENV["BUILD_NUMBER"]
	"#{$major_version}.#{yearversion}.#{dateversion}.#{buildnumber}"
end
